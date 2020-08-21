#ifndef INC_CHIP_YM2612
#define INC_CHIP_YM2612

#include <cstdint>

class Ym2612
{
public:
    static void setup(void);
    static void pushData(uint8_t, uint8_t);
};

#endif
