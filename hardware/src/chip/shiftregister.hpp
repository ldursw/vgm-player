#ifndef INC_CHIP_COMMON
#define INC_CHIP_COMMON

#include <cstdint>

class ShiftRegister {
public:
    static void setup(void);
    static void pushData(uint8_t);

private:
    inline static void pushBit(uint8_t);

    static bool _setup;
};

#endif
