// SPDX-License-Identifier: GPL-3.0
#if ARDUINO_RASPBERRY_PI_PICO

#include "timer.hpp"

#include <pico/time.h>

repeating_timer_t _timer;
void (*_timer_callback)();

static bool onTime(repeating_timer_t *rt)
{
    _timer_callback();

    return true;
}

void Timer::begin(void (*callback)())
{
    _timer_callback = callback;
    auto pool = alarm_pool_create(3, 1);
    alarm_pool_add_repeating_timer_us(pool, sampleTicks, &onTime, nullptr, &_timer);
}

#endif
