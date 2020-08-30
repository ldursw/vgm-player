#include <Arduino.h>
#include "timer.hpp"

#if TEENSYDUINO
static IntervalTimer _timer;
#endif

#if ARDUINO_SAM_DUE
#include <DueTimer.hpp>
#endif

void Timer::begin(void (*callback)())
{
#if TEENSYDUINO
    _timer.begin(callback, sampleTicks);
#elif ARDUINO_SAM_DUE
    Timer3.attachInterrupt(callback).start(sampleTicks);
#else
    #error Missing implementation for callback timer
#endif
}
