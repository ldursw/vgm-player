#ifndef INC_UTIL_HAL
#define INC_UTIL_HAL

#if ARDUINO_SAM_DUE

#define SdFatCls SdFat
#define digitalWriteFast digitalWrite
#define analogWriteDAC0(value) analogWrite(DAC0, value)
#define PSG_WE 29
#define PSG_DAC DAC0
#define FM_IC 28
#define FM_WR 27
#define FM_A0 26
#define FM_A1 25

#elif TEENSYDUINO

#define SdFatCls SdFatSdio
#define PSG_WE 29
#define PSG_DAC A21
#define FM_IC 28
#define FM_WR 27
#define FM_A0 26
#define FM_A1 25

#endif

#endif
