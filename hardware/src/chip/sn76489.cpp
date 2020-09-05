// SPDX-License-Identifier: GPL-3.0
#include <Arduino.h>
#include "sn76489.hpp"
#include "shiftregister.hpp"
#include "util/hal.hpp"

#ifndef USE_REAL_PSG
#include "psgEmu.hpp"
#endif

void Sn76489::setup(void)
{
#ifdef USE_REAL_PSG
    ShiftRegister::setup();
    pinMode(PSG_WE, OUTPUT);
#else
    EmulatedPsg::reset();
    pinMode(PSG_DAC, OUTPUT);
    analogWriteResolution(12);
    enableDAC0();
#endif

    // silence PSG

    // channel 0 volume mute
    pushData(0x9f);
    // channel 0 tone mute
    pushData(0x80);
    pushData(0x00);
    // channel 1 volume mute
    pushData(0xbf);
    // channel 1 tone mute
    pushData(0xa0);
    pushData(0x00);
    // channel 2 volume mute
    pushData(0xdf);
    // channel 2 tone mute
    pushData(0xc0);
    pushData(0x00);
    // channel 3 volume mute
    pushData(0xff);
    // channel 3 tone mute
    pushData(0xe0);
    pushData(0x00);
}

void Sn76489::pushData(uint8_t data)
{
#ifdef USE_REAL_PSG
    ShiftRegister::pushData(data);

    digitalWriteFast(PSG_WE, LOW);
    wait10ns();
    digitalWriteFast(PSG_WE, HIGH);
    wait10ns();
#else
    EmulatedPsg::write(data);
#endif
}

void Sn76489::update(void)
{
#ifndef USE_REAL_PSG
    // divide value to reduce the volume
    int32_t sample = EmulatedPsg::getSample() / PSG_ATTENUATION;
    // add offset for bipolar output
    analogWriteDAC0(sample + 2048);
#endif
}

void Sn76489::wait10ns(void)
{
    for (uint8_t i = 0; i < 100; i++)
    {
        asm("nop\nnop\nnop\nnop\nnop\nnop\nnop\n");
        asm("nop\nnop\nnop\nnop\nnop\nnop\nnop\n");
        asm("nop\nnop\nnop\nnop\nnop\nnop\nnop\n");
        asm("nop\nnop\nnop\nnop\nnop\nnop\nnop\n");
        asm("nop\nnop\nnop\nnop\nnop\n");
    }
}
