// SPDX-License-Identifier: GPL-3.0
#if ARDUINO_SAM_DUE

#include <Arduino.h>
#include "timer.hpp"

#include <DueTimer.hpp>

void Timer::begin(void (*callback)())
{
    Timer3.attachInterrupt(callback).start(sampleTicks);
}

#endif
