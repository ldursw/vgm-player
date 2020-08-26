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

    for (int8_t i = 7; i >= 0; i--)
    {
        digitalWriteFast(SR_DATA, data & (1 << i));
        digitalWriteFast(SR_CLOCK, HIGH);
        asm("nop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\n");
        digitalWriteFast(SR_CLOCK, LOW);
    }

    digitalWriteFast(SR_LATCH, HIGH);
    asm("nop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\n");
}
