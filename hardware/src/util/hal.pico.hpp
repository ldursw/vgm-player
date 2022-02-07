#define SdFatCls SdFat
#define PSG_WE 22
#define PSG_ATTENUATION 4
#define FM_IC 18
#define FM_WR 19
#define FM_A0 20
#define FM_A1 21

#include <Arduino.h>
#include <stdio.h>

inline __attribute__((always_inline)) void digitalWriteFast(uint8_t pin, uint8_t val)
{
    digitalWrite(pin, val);
}

inline __attribute__((always_inline)) void enableDAC0()
{
}

inline __attribute__((always_inline)) void analogWriteDAC0(uint32_t sample)
{
}
