#include <Arduino.h>
#include "player/dispatcher.hpp"
#include "util/hal.hpp"

#if defined(ENABLE_PLAYER) && defined(ENABLE_STREAM)
#error Player and Stream cannot be activated at the same time
#elif !defined(ENABLE_PLAYER) && !defined(ENABLE_STREAM)
#error Please specify either "ENABLE_PLAYER" or "ENABLE_STREAM"
#endif

#ifdef ENABLE_PLAYER
#include "player/vgmplayer.hpp"
#endif

void setup(void)
{
    // Activity LED
    pinMode(13, OUTPUT);

    Dispatcher::setup();

#ifdef ENABLE_STREAM
    SerialUSB.begin(115200);
#endif

#ifdef ENABLE_PLAYER
    delay(1000);

    if (!VgmPlayer::setup("music.vgm"))
    {
        digitalWriteFast(13, HIGH);

        return;
    }
#endif
}

void loop(void)
{
#ifdef ENABLE_STREAM
    if (Dispatcher::isBufferFull())
    {
        return;
    }

    char buf[3];
    uint8_t index = 0;
    while (index < sizeof(buf))
    {
        index += SerialUSB.readBytes(buf + index, sizeof(buf) - index);
    }

    Dispatcher::enqueue(Instruction(buf[0], buf[1], buf[2]));
#endif
}
