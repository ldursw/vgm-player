#include "clock.hpp"
#include "Adafruit_SI5351.hpp"

void ChipClock::setup()
{
    auto clockgen = Adafruit_SI5351();
    clockgen.begin();

    // Setup clock for SN76489
#ifdef USE_REAL_PSG
    /*
    PLL A
        Input Frequency (MHz) = 25.000000000
        VCO Frequency (MHz) =  737.386200000
        Feedback Divider = 29  61931/125000
        SSC disabled
    Channel 2
        Output Frequency (MHz) = 3.579545000
        Multisynth Output Frequency (MHz) = 3.579545000
        Multisynth Divider = 206
        R Divider = 1
        PLL source = PLLA
        Initial phase offset (ns) = 0.000
        Powered down = No
        Inverted = No
        Drive Strength = b11
        Disable State = Low
        Clock Source = b11
    */
    clockgen.setupPLL(SI5351_PLL_A, 29, 61931, 125000);
    clockgen.setupMultisynth(2, SI5351_PLL_A, 206, 0, 1);
#endif

    // Setup clock for YM2612
    /*
    PLL B
        Input Frequency (MHz) = 25.000000000
        VCO Frequency (MHz) =  705.681600000
        Feedback Divider = 28  3551/15625
    Channel 0
        Output Frequency (MHz) = 7.670453000
        Multisynth Output Frequency (MHz) = 7.670453000
        Multisynth Divider = 92
        R Divider = 1
        PLL source = PLLB
        Initial phase offset (ns) = 0.000
        Powered down = No
        Inverted = No
        Drive Strength = b11
        Disable State = Low
        Clock Source = b11
    */
    clockgen.setupPLL(SI5351_PLL_B, 28, 3551, 15625);
    clockgen.setupMultisynth(0, SI5351_PLL_B, 92, 0, 1);

    clockgen.enableOutputs(true);
    clockgen.end();
}
