// SPDX-License-Identifier: GPL-3.0
#include "psgEmu.hpp"
#include "util/fixed.hpp"

static const fpm::fixed_16_16 SampleClock { 3579545.0f / 16.0f / 44100.0f };

fpm::fixed_16_16 EmulatedPsg::_clock;
int32_t EmulatedPsg::_clocksForSample;
int32_t EmulatedPsg::_registers[8];
int32_t EmulatedPsg::_latchedRegister;
int32_t EmulatedPsg::_noiseShiftRegister;
int32_t EmulatedPsg::_noiseFreq;
int32_t EmulatedPsg::_toneFreqVals[4];
int32_t EmulatedPsg::_toneFreqPos[4];
int32_t EmulatedPsg::_channels[4];
fpm::fixed_16_16 EmulatedPsg::_intermediatePos[4];
bool EmulatedPsg::_antiAliasing[4];
const int32_t EmulatedPsg::VolumeValues[16] =
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
        _antiAliasing[i] = false;
    }

    _latchedRegister = 0;

    // Initialise noise generator
    _noiseShiftRegister = NoiseInitialState;

    // Zero clock
    _clock *= 0;
}

void EmulatedPsg::write(uint8_t data)
{
    if ((data & 0x80) > 0)
    {
        // Latch/data byte %1 cc t dddd
        // zero low 4 bits and replace with data
        _latchedRegister = (data >> 4) & 0x07;
        _registers[_latchedRegister] =
            (_registers[_latchedRegister] & 0x3f0) | (data & 0xf);
    }
    else if (!(_latchedRegister % 2 > 0) && (_latchedRegister < 5))
    {
        // Data byte %0 - dddddd
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

    if (_latchedRegister == 6)
    {
        // Noise channel (register 6)
        // reset shift register
        _noiseShiftRegister = NoiseInitialState;
        // set noise signal generator frequency
        _noiseFreq = 0x10 << (_registers[6] & 0x3);
    }
    else
    {
        // Tone channels (register 0, 2, 4)
        if (_registers[_latchedRegister] == 0)
        {
            // Zero frequency changed to 1 to avoid div/0
            _registers[_latchedRegister] = 1;
        }
    }
}

int32_t EmulatedPsg::getSample(void)
{
    calculateToneChannel();
    calculateNoiseChannel();

    // Build result into buffer for all 4 channels
    int32_t sample = _channels[0] + _channels[1] + _channels[2] + _channels[3];

    incrementClock();

    return sample;
}

void EmulatedPsg::calculateToneChannel(void)
{
    for (uint8_t i = 0; i <= 2; ++i)
    {
        updateToneChannel(i);

        int32_t volume = VolumeValues[_registers[2 * i + 1]];
        if (_antiAliasing[i])
        {
            // Intermediate position (antialiasing)
            _channels[i] = static_cast<int>(volume * _intermediatePos[i]);
        }
        else
        {
            // Flat (no antialiasing needed)
            _channels[i] = volume * _toneFreqPos[i];
        }
    }
}

void EmulatedPsg::calculateNoiseChannel(void)
{
    updateNoiseChannel();

    int32_t volume = VolumeValues[_registers[7]];
    // double noise volume
    // _channels[3] = volume * (_noiseShiftRegister & 0x1) * 2;

    // Now the noise is bipolar, too. -Valley Bell
    _channels[3] = volume * ((_noiseShiftRegister & 0x1) * 2 - 1);

    // due to the way the white noise works here, it seems twice as loud as it should be
    // _channels[3] >>= (_registers[6] & 0x4) >> 2;
}

void EmulatedPsg::incrementClock(void)
{
    // Increment clock by 1 sample length
    _clock += SampleClock;
    _clocksForSample = static_cast<int>(_clock); // truncate
    _clock -= _clocksForSample;     // remove integer part

    // Decrement tone channel counters
    _toneFreqVals[0] -= _clocksForSample;
    _toneFreqVals[1] -= _clocksForSample;
    _toneFreqVals[2] -= _clocksForSample;

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

void EmulatedPsg::updateToneChannel(uint8_t i)
{
    if (_toneFreqVals[i] >= 0)
    {
        // signal no antialiasing needed
        _antiAliasing[i] = false;

        return;
    }

    // If the counter gets below 0...
    if (_registers[i * 2] >= Cutoff)
    {
        // For tone-generating values, calculate how much of
        // the sample is + and how much is -
        // This is optimised into an even more confusing state
        // than it was in the first place...
        _intermediatePos[i] = (_clocksForSample - _clock + 2 * _toneFreqVals[i]) *
            _toneFreqPos[i] / (_clocksForSample + _clock);
        // Flip the flip-flop
        _toneFreqPos[i] = -_toneFreqPos[i];
        _antiAliasing[i] = true;
    }
    else
    {
        // stuck value
        _toneFreqPos[i] = 1;
        _antiAliasing[i] = false;
    }

    _toneFreqVals[i] += _registers[i * 2] *
        (_clocksForSample / _registers[i * 2] + 1);
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
        _toneFreqVals[3] += _noiseFreq * (_clocksForSample / _noiseFreq + 1);
    }

    if (_toneFreqPos[3] == 1)
    {
        // On the positive edge of the square wave (only once per cycle)
        int32_t feedback;
        if ((_registers[6] & 0x4) > 0)
        {
            // White noise
            // Calculate parity of fed-back bits for feedback. If two
            // bits fed back, I can do
            // Feedback=(nsr & fb) && (nsr & fb ^ fb)
            // since that's (one or more bits set) && (not all bits set)
            feedback = ((_noiseShiftRegister & 9) > 0 &&
                ((_noiseShiftRegister & 9) ^ 9) > 0) ? 1 : 0;
        }
        else
        {
            // Periodic noise
            feedback = _noiseShiftRegister & 1;
        }

        _noiseShiftRegister = (_noiseShiftRegister >> 1) | (feedback << (16 - 1));
    }
}
