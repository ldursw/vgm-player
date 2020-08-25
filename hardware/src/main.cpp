#include <Arduino.h>
#include "player/dispatcher.hpp"

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
    Serial.begin(115200);
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

    Serial.rts();

    char buf[3];
    uint8_t index = 0;
    while (index < sizeof(buf))
    {
        index += Serial.readBytes(buf + index, sizeof(buf) - index);
    }

    uint8_t command = buf[0];
    uint8_t data1 = buf[1];
    uint8_t data2 = buf[2];

    Dispatcher::enqueue(command, data1, data2);
#endif
}
