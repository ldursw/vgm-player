// SPDX-License-Identifier: GPL-3.0
#include "shiftregister.hpp"
#include <Arduino.h>
#include <SPI.h>
#include "util/hal.hpp"

bool ShiftRegister::_setup = false;
static SPISettings _srSpiSettings(8'000'000, MSBFIRST, SPI_MODE0);

void ShiftRegister::setup(void)
{
    if (_setup)
    {
        return;
    }

    pinMode(MOSI, OUTPUT);
    pinMode(SCK, OUTPUT);
    pinMode(SS, OUTPUT);

    digitalWriteFast(MOSI, LOW);
    digitalWriteFast(SCK, LOW);
    digitalWriteFast(SS, LOW);

    SPI.begin();

    _setup = true;
}

void ShiftRegister::pushData(uint8_t data)
{
    digitalWriteFast(SS, LOW);

    SPI.beginTransaction(_srSpiSettings);
    SPI.transfer(data);
    SPI.endTransaction();

    digitalWriteFast(SS, HIGH);
}
