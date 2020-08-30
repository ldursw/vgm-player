// SPDX-License-Identifier: GPL-3.0
#ifndef INC_CHIP_COMMON
#define INC_CHIP_COMMON

#include <cstdint>

class ShiftRegister {
public:
    static void setup(void);
    static void pushData(uint8_t);

private:
    static bool _setup;
};

#endif
