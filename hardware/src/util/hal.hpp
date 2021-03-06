// SPDX-License-Identifier: GPL-3.0
#ifndef INC_UTIL_HAL
#define INC_UTIL_HAL

#if ARDUINO_SAM_DUE
# include "hal.due.hpp"
#elif TEENSYDUINO
# include "hal.teensy36.hpp"
#elif ARDUINO_RASPBERRY_PI_PICO
# include "hal.pico.hpp"
#else
# error "HAL not found for this architecture"
#endif

#endif
