#include "psgEmu.hpp"

float EmulatedPsg::_clock;
int EmulatedPsg::_clocksForSample;
int EmulatedPsg::_registers[8];
int EmulatedPsg::_latchedRegister;
int EmulatedPsg::_noiseShiftRegister;
int EmulatedPsg::_noiseFreq;
int EmulatedPsg::_toneFreqVals[4];
int EmulatedPsg::_toneFreqPos[4];
int EmulatedPsg::_channels[4];
float EmulatedPsg::_intermediatePos[4];
const int EmulatedPsg::VolumeValues[16] =
{
    // These values are taken from a real SMS2's output
    // I can't remember why 892... :P some scaling I did at some point
    892, 892, 892, 760, 623, 497, 404, 323, 257, 198, 159, 123, 96, 75, 60, 0
};

void EmulatedPsg::reset(void)
{
    for (uint8_t i = 0; i <= 3; i++)
    {
        // Initialise PSG state
        _registers[2 * i] = 1;       // tone freq=1
        _registers[2 * i + 1] = 0xf; // vol=off
        _noiseFreq = 0x10;

        // Set counters to 0
        _toneFreqVals[i] = 0;

        // Set flip-flops to 1
        _toneFreqPos[i] = 1;

        // Set intermediate positions to do-not-use value
        _intermediatePos[i] = FloatMin;
    }

    _latchedRegister = 0;

    // Initialise noise generator
    _noiseShiftRegister = NoiseInitialState;

    // Zero clock
    _clock = 0;
}

void EmulatedPsg::write(uint8_t data)
{
    // Latch/data byte  %1 cc t dddd
    if ((data & 0x80) > 0)
    {
        // zero low 4 bits and replace with data
        _latchedRegister = (data >> 4) & 0x07;
        _registers[_latchedRegister] =
            (_registers[_latchedRegister] & 0x3f0) | (data & 0xf);
    }
    // Data byte        %0 - dddddd
    else if (!(_latchedRegister % 2 > 0) && (_latchedRegister < 5))
    {
        // Tone register
        // zero high 6 bits and replace with data
        _registers[_latchedRegister] =
            (_registers[_latchedRegister] & 0x00f) | ((data & 0x3f) << 4);
    }
    else
    {
        // Other register
        // Replace with data
        _registers[_latchedRegister] = data & 0x0f;
    }

    switch (_latchedRegister)
    {
        case 0:
        case 2:
        case 4: // Tone channels
            if (_registers[_latchedRegister] == 0)
            {
                // Zero frequency changed to 1 to avoid div/0
                _registers[_latchedRegister] = 1;
            }
            break;
        case 6: // Noise
            // reset shift register
            _noiseShiftRegister = NoiseInitialState;
            // set noise signal generator frequency
            _noiseFreq = 0x10 << (_registers[6] & 0x3);
            break;
    }
}

int32_t EmulatedPsg::getSample(void)
{
    calculateToneChannel();
    calculateNoiseChannel();

    // Build result into buffer for all 4 channels
    int32_t sample = _channels[0] + _channels[1] + _channels[2] + _channels[3];

    incrementClock();
    updateToneChannel();
    updateNoiseChannel();

    return sample;
}

void EmulatedPsg::calculateToneChannel(void)
{
    for (int i = 0; i <= 2; ++i)
    {
        if (_intermediatePos[i] != FloatMin)
        {
            // Intermediate position (antialiasing)
            _channels[i] = (short)(VolumeValues[_registers[2 * i + 1]] *
                _intermediatePos[i]);
        }
        else
        {
            // Flat (no antialiasing needed)
            _channels[i] = VolumeValues[_registers[2 * i + 1]] *
                _toneFreqPos[i];
        }
    }
}

void EmulatedPsg::calculateNoiseChannel(void)
{
    // double noise volume
    // _channels[3] = PSGVolumeValues[_registers[7]] * (_noiseShiftRegister & 0x1) * 2;

    // Now the noise is bipolar, too. -Valley Bell
    _channels[3] = VolumeValues[_registers[7]] * ((_noiseShiftRegister & 0x1) * 2 - 1);

    // due to the way the white noise works here, it seems twice as loud as it should be
    // _channels[3] >>= (_registers[6] & 0x4) >> 2;
}

void EmulatedPsg::incrementClock(void)
{
    // Increment clock by 1 sample length
    _clock += SampleClock;
    _clocksForSample = (int)_clock; // truncate
    _clock -= _clocksForSample;     // remove integer part

    // Decrement tone channel counters
    for (uint8_t i = 0; i <= 2; ++i)
    {
        _toneFreqVals[i] -= _clocksForSample;
    }

    // Noise channel: match to tone2 or decrement its counter
    if (_noiseFreq == 0x80)
    {
        _toneFreqVals[3] = _toneFreqVals[2];
    }
    else
    {
        _toneFreqVals[3] -= _clocksForSample;
    }
}

void EmulatedPsg::updateToneChannel(void)
{
    for (uint8_t i = 0; i <= 2; ++i)
    {
        if (_toneFreqVals[i] <= 0)
        {
            // If the counter gets below 0...
            if (_registers[i * 2] >= Cutoff)
            {
                // For tone-generating values, calculate how much of
                // the sample is + and how much is -
                // This is optimised into an even more confusing state
                // than it was in the first place...
                _intermediatePos[i] = (_clocksForSample - _clock + 2 *
                    _toneFreqVals[i]) * _toneFreqPos[i] / (_clocksForSample + _clock);
                // Flip the flip-flop
                _toneFreqPos[i] = -_toneFreqPos[i];
            }
            else
            {
                // stuck value
                _toneFreqPos[i] = 1;
                _intermediatePos[i] = FloatMin;
            }

            _toneFreqVals[i] += _registers[i * 2] *
                (_clocksForSample / _registers[i * 2] + 1);
        }
        else
        {
            // signal no antialiasing needed
            _intermediatePos[i] = FloatMin;
        }
    }
}

void EmulatedPsg::updateNoiseChannel(void)
{
    if (_toneFreqVals[3] > 0)
    {
        return;
    }

    // If the counter gets below 0...
    // Flip the flip-flop
    _toneFreqPos[3] = -_toneFreqPos[3];
    if (_noiseFreq != 0x80)
    {
        // If not matching tone2, decrement counter
        _toneFreqVals[3] += _noiseFreq *
            (_clocksForSample / _noiseFreq + 1);
    }

    if (_toneFreqPos[3] == 1)
    {
        // On the positive edge of the square wave (only once per cycle)
        int Feedback;
        if ((_registers[6] & 0x4) > 0)
        {
            // White noise */
            // Calculate parity of fed-back bits for feedback. If two
            // bits fed back, I can do
            // Feedback=(nsr & fb) && (nsr & fb ^ fb)
            // since that's (one or more bits set) && (not all bits set)
            Feedback = ((_noiseShiftRegister & 9) > 0 &&
                ((_noiseShiftRegister & 9) ^ 9) > 0) ? 1 : 0;
        }
        else
        {
            // Periodic noise
            Feedback = _noiseShiftRegister & 1;
        }

        _noiseShiftRegister = (_noiseShiftRegister >> 1) | (Feedback << (16 - 1));
    }
}
