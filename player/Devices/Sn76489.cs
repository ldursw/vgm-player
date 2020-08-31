// SPDX-License-Identifier: GPL-3.0
namespace VgmPlayer.Devices
{
    // Ported from VGMPlay
    class Sn76489
    {
        private const float FloatMin = 1.175494351e-38F;
        // Initial state of shift register
        private const int NoiseInitialState = 0x8000;
        // Value below which PSG does not output
        private const int Cutoff = 0x6;
        // Sample clock
        private const float SampleClock = 3579545f / 16f / 44100f;
        // The MAME core uses 0x2000 as maximum volume (0x1000 for bipolar output)
        private static readonly int[] PSGVolumeValues =
        {
            4096, 3254, 2584, 2053, 1631, 1295, 1029,
            817, 649, 516, 410, 325, 258, 205, 163, 0
        };

        // per-channel muting
        // public MuteValues Mute { get; set; } = MuteValues.AllOn;

        // Variables
        private float _clock;
        private int _clocksForSample;

        // PSG registers:
        // Tone, vol x4
        private readonly int[] _registers = new int[8];
        private int _latchedRegister;
        private int _noiseShiftRegister;
        // Noise channel signal generator frequency
        private int _noiseFreq;

        // Output calculation variables
        // Frequency register values (counters)
        private readonly int[] _toneFreqVals = new int[4];
        // Frequency channel flip-flops
        private readonly int[] _toneFreqPos = new int[4];
        // Value of each channel, before stereo is applied
        private readonly int[] _channels = new int[4];
        // intermediate values used at boundaries between + and -
        // (does not need double accuracy)
        private readonly float[] _intermediatePos = new float[4];

        public Sn76489()
        {
            Reset();
        }

        public void Reset()
        {
            for (var i = 0; i <= 3; i++)
            {
                /* Initialise PSG state */
                _registers[2 * i] = 1;      /* tone freq=1 */
                _registers[2 * i + 1] = 0xf;    /* vol=off */
                _noiseFreq = 0x10;

                /* Set counters to 0 */
                _toneFreqVals[i] = 0;

                /* Set flip-flops to 1 */
                _toneFreqPos[i] = 1;

                /* Set intermediate positions to do-not-use value */
                _intermediatePos[i] = FloatMin;

                /* Set panning to centre */
                //centre_panning( panning[i] );
            }

            _latchedRegister = 0;

            /* Initialise noise generator */
            _noiseShiftRegister = NoiseInitialState;

            /* Zero clock */
            _clock = 0;
        }

        public void Write(int data)
        {
            /* Latch/data byte  %1 cc t dddd */
            if ((data & 0x80) > 0)
            {
                /* zero low 4 bits and replace with data */
                _latchedRegister = (data >> 4) & 0x07;
                _registers[_latchedRegister] =
                    (_registers[_latchedRegister] & 0x3f0)
                    | (data & 0xf);
            }
            /* Data byte        %0 - dddddd */
            else if (!(_latchedRegister % 2 > 0) && (_latchedRegister < 5))
            {
                /* Tone register */
                /* zero high 6 bits and replace with data */
                _registers[_latchedRegister] =
                    (_registers[_latchedRegister] & 0x00f) | ((data & 0x3f) << 4);
            }
            else
            {
                /* Other register */
                /* Replace with data */
                _registers[_latchedRegister] = data & 0x0f;
            }

            switch (_latchedRegister)
            {
                case 0:
                case 2:
                case 4: /* Tone channels */
                    if (_registers[_latchedRegister] == 0)
                    {
                        /* Zero frequency changed to 1 to avoid div/0 */
                        _registers[_latchedRegister] = 1;
                    }
                    break;
                case 6: /* Noise */
                    /* reset shift register */
                    _noiseShiftRegister = NoiseInitialState;
                    /* set noise signal generator frequency */
                    _noiseFreq = 0x10 << (_registers[6] & 0x3);
                    break;
            }
        }

        public void Update(int[] buffer, int length)
        {
            for (int j = 0; j < length; j++)
            {
                buffer[j] = GetSample();
            }
        }

        public int GetSample()
        {
            CalculateToneChannel();
            CalculateNoiseChannel();

            // Build result into buffer for all 4 channels
            var sample = _channels[0] + _channels[1] + _channels[2] + _channels[3];

            IncrementClock();
            UpdateToneChannel();
            UpdateNoiseChannel();

            return sample;
        }

        private void CalculateToneChannel()
        {
            /*for (int i = 0; i <= 2; ++i)
            {
                if (((int)Mute & (1 << i)) > 0)
                {
                    if (_intermediatePos[i] != FloatMin)
                    {
                        // Intermediate position (antialiasing)
                        _channels[i] = (short)(
                            PSGVolumeValues[_registers[2 * i + 1]] *
                            _intermediatePos[i]);
                    }
                    else
                    {
                        // Flat (no antialiasing needed)
                        _channels[i] = PSGVolumeValues[_registers[2 * i + 1]] *
                            _toneFreqPos[i];
                    }
                }
                else
                {
                    // Muted channel
                    _channels[i] = 0;
                }
            }*/

            CalculateToneChannel(0);
            CalculateToneChannel(1);
            CalculateToneChannel(2);
        }

        private void CalculateToneChannel(int index)
        {
            if (_intermediatePos[index] != FloatMin)
            {
                /* Intermediate position (antialiasing) */
                _channels[index] = (short)(
                    PSGVolumeValues[_registers[2 * index + 1]] *
                    _intermediatePos[index]);
            }
            else
            {
                /* Flat (no antialiasing needed) */
                _channels[index] = PSGVolumeValues[_registers[2 * index + 1]] *
                    _toneFreqPos[index];
            }
        }

        private void CalculateNoiseChannel()
        {
            _channels[3] = PSGVolumeValues[_registers[7]] *
                (_noiseShiftRegister & 0x1) * 2;
            /*if ((Mute & MuteValues.Noise) > 0)
            {
                _channels[3] = PSGVolumeValues[_registers[7]] *
                    (_noiseShiftRegister & 0x1) * 2;
            }
            else
            {
                _channels[3] = 0;
            }*/
        }

        private void IncrementClock()
        {
            /* Increment clock by 1 sample length */
            _clock += SampleClock;
            _clocksForSample = (int)_clock;  /* truncate */
            _clock -= _clocksForSample;      /* remove integer part */

            /* Decrement tone channel counters */
            _toneFreqVals[0] -= _clocksForSample;
            _toneFreqVals[1] -= _clocksForSample;
            _toneFreqVals[2] -= _clocksForSample;

            /* Noise channel: match to tone2 or decrement its counter */
            if (_noiseFreq == 0x80)
            {
                _toneFreqVals[3] = _toneFreqVals[2];
            }
            else
            {
                _toneFreqVals[3] -= _clocksForSample;
            }
        }

        private void UpdateToneChannel()
        {
            for (var i = 0; i <= 2; ++i)
            {
                if (_toneFreqVals[i] > 0)
                {
                    /* signal no antialiasing needed */
                    _intermediatePos[i] = FloatMin;
                }
                else
                {
                    /* If the counter gets below 0... */
                    if (_registers[i * 2] >= Cutoff)
                    {
                        // For tone-generating values, calculate how much of
                        // the sample is + and how much is -
                        // This is optimised into an even more confusing state
                        // than it was in the first place...
                        _intermediatePos[i] = (_clocksForSample - _clock + 2 *
                            _toneFreqVals[i]) * _toneFreqPos[i] /
                            (_clocksForSample + _clock);
                        /* Flip the flip-flop */
                        _toneFreqPos[i] = -_toneFreqPos[i];
                    }
                    else
                    {
                        /* stuck value */
                        _toneFreqPos[i] = 1;
                        _intermediatePos[i] = FloatMin;
                    }

                    _toneFreqVals[i] += _registers[i * 2] *
                        (_clocksForSample / _registers[i * 2] + 1);
                }
            }
        }

        private void UpdateNoiseChannel()
        {
            if (_toneFreqVals[3] > 0)
            {
                return;
            }

            /* If the counter gets below 0... */
            /* Flip the flip-flop */
            _toneFreqPos[3] = -_toneFreqPos[3];
            if (_noiseFreq != 0x80)
            {
                /* If not matching tone2, decrement counter */
                _toneFreqVals[3] += _noiseFreq *
                    (_clocksForSample / _noiseFreq + 1);
            }

            if (_toneFreqPos[3] == 1)
            {
                // On the positive edge of the square wave (only once per cycle)
                int feedback;
                if ((_registers[6] & 0x4) > 0)
                {
                    // White noise */
                    // Calculate parity of fed-back bits for feedback. If two
                    // bits fed back, I can do
                    // Feedback=(nsr & fb) && (nsr & fb ^ fb)
                    // since that's (one or more bits set) && (not all bits set)
                    feedback =
                        ((_noiseShiftRegister & 9) > 0 &&
                        ((_noiseShiftRegister & 9) ^ 9) > 0) ? 1 : 0;
                }
                else
                {
                    /* Periodic noise */
                    feedback = _noiseShiftRegister & 1;
                }

                _noiseShiftRegister = (_noiseShiftRegister >> 1) |
                    (feedback << (16 - 1));
            }
        }

        public enum MuteValues : int
        {
            AllOff = 0,
            Tone1 = 1,
            Tone2 = 2,
            Tone3 = 4,
            Noise = 8,
            Tones = Tone1 | Tone2 | Tone3,
            AllOn = Tones | Noise,
        }
    }
}
