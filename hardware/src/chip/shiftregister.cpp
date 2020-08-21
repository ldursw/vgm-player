#include "shiftregister.hpp"
#include <Arduino.h>

#define SR_DATA 32
#define SR_LATCH 31
#define SR_CLOCK 30

bool ShiftRegister::_setup = false;

void ShiftRegister::setup(void)
{
    if (_setup)
    {
        return;
    }

    pinMode(SR_DATA, OUTPUT);
    pinMode(SR_CLOCK, OUTPUT);
    pinMode(SR_LATCH, OUTPUT);

    digitalWriteFast(SR_DATA, LOW);
    digitalWriteFast(SR_CLOCK, LOW);
    digitalWriteFast(SR_LATCH, LOW);

    _setup = true;
}

void ShiftRegister::pushData(uint8_t data)
{
    // We have to bit-bang the shift register because
    // the SPI library doesn't work in interrupts

    digitalWriteFast(SR_LATCH, LOW);

    pushBit((data >> 7) & 0x01);
    pushBit((data >> 6) & 0x01);
    pushBit((data >> 5) & 0x01);
    pushBit((data >> 4) & 0x01);
    pushBit((data >> 3) & 0x01);
    pushBit((data >> 2) & 0x01);
    pushBit((data >> 1) & 0x01);
    pushBit((data >> 0) & 0x01);

    digitalWriteFast(SR_LATCH, HIGH);
    asm("nop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\n");
}

__attribute__((always_inline)) void ShiftRegister::pushBit(uint8_t data)
{
    digitalWriteFast(SR_DATA, data);
    digitalWriteFast(SR_CLOCK, HIGH);
    asm("nop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\n");
    digitalWriteFast(SR_CLOCK, LOW);
}
