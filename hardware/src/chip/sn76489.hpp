// SPDX-License-Identifier: GPL-3.0
#ifndef INC_CHIP_SN76489
#define INC_CHIP_SN76489

#include <cstdint>

class Sn76489
{
public:
    static void setup(void);
    static void pushData(uint8_t);
    static void update(void);

private:
    static void wait10ns(void);
};

#endif
