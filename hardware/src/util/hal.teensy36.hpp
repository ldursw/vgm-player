#define SdFatCls SdFatSdio
#define SerialUSB Serial

#define PSG_DAC A21
#define PSG_ATTENUATION 6

#define FM_A1   25
#define FM_A0   26
#define FM_WR   27
#define FM_IC   28
#define PSG_WE  29
#define PSG_RDY 30
#define FM_RD   31
#define FM_D7   32

#include <Arduino.h>

inline __attribute__((always_inline)) void enableDAC0()
{
    pinMode(PSG_DAC, OUTPUT);
    analogWriteResolution(12);
}
