// SPDX-License-Identifier: GPL-3.0
#if TEENSYDUINO

#include <Arduino.h>
#include "timer.hpp"

static IntervalTimer _timer;

void Timer::begin(void (*callback)())
{
    _timer.begin(callback, sampleTicks);
}

#endif
