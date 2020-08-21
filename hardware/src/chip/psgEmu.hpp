#ifndef INC_CHIP_PSGNOISE
#define INC_CHIP_PSGNOISE

#include <cstdint>

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

    // Constants
    static constexpr float FloatMin = 1.175494351e-38F;
    // Initial state of shift register
    static constexpr int NoiseInitialState = 0x8000;
    // Value below which PSG does not output
    static constexpr int Cutoff = 0x6;
    // Sample clock
    static constexpr float SampleClock = 3579545.0 / 16.0 / 44100.0;
    // Volume values
    static const int VolumeValues[16];

    // Clock ticks
    static float _clock;
    static int _clocksForSample;

    // PSG registers:
    // Tone, vol x4
    static int _registers[8];
    static int _latchedRegister;
    static int _noiseShiftRegister;
    // Noise channel signal generator frequency
    static int _noiseFreq;

    // Output calculation variables
    // Frequency register values (counters)
    static int _toneFreqVals[4];
    // Frequency channel flip-flops
    static int _toneFreqPos[4];
    // Value of each channel, before stereo is applied
    static int _channels[4];
    // intermediate values used at boundaries between + and -
    // (does not need double accuracy)
    static float _intermediatePos[4];
};

#endif
