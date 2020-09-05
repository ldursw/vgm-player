// SPDX-License-Identifier: GPL-3.0
#ifndef INC_CHIP_PSGNOISE
#define INC_CHIP_PSGNOISE

#include <cstdint>

// FLoat or INT. Prefer int when FPU is unavailable
#if __SOFTFP__
typedef int32_t flint;
#else
typedef float flint;
#endif

// SN76489 emulation by Maxim in 2001 and 2002
// Taken from VGMPlay source
class EmulatedPsg
{
public:
    static void reset(void);
    static void write(uint8_t data);
    static int32_t getSample(void);

private:
    static void calculateToneChannel(void);
    static void calculateNoiseChannel(void);
    static void incrementClock(void);
    static void updateToneChannel(void);
    static void updateNoiseChannel(void);

    // Initial state of shift register
    static constexpr int32_t NoiseInitialState = 0x8000;
    // Value below which PSG does not output
    static constexpr int32_t Cutoff = 0x6;
    // Sample clock
    static constexpr flint SampleClock = 3579545.0f / 16.0f / 44100.0f;
    // Volume values
    static const int32_t VolumeValues[16];

    // Clock ticks
    static flint _clock;
    static int32_t _clocksForSample;

    // PSG registers:
    // Tone, vol x4
    static int32_t _registers[8];
    static int32_t _latchedRegister;
    static int32_t _noiseShiftRegister;
    // Noise channel signal generator frequency
    static int32_t _noiseFreq;

    // Output calculation variables
    // Frequency register values (counters)
    static int32_t _toneFreqVals[4];
    // Frequency channel flip-flops
    static int32_t _toneFreqPos[4];
    // Value of each channel, before stereo is applied
    static int32_t _channels[4];
    // intermediate values used at boundaries between + and -
    // (does not need double accuracy)
    static flint _intermediatePos[4];
    static bool _antiAliasing[4];
};

#endif
