#ifndef INC_PLAYER_DISPATCHER
#define INC_PLAYER_DISPATCHER

#include <cstdint>
#include <Arduino.h>
#include "../util/buffer.hpp"

#define DISPATCHER_BUFFER_SIZE 16

class Dispatcher {
public:
    static void enqueue(uint8_t command, uint8_t data1, uint8_t data2);
    static void processImmediate(uint8_t command, uint8_t data1, uint8_t data2);
    static void setup(void);
    static void process(void);

    static bool isBufferFull(void)
    {
        return _buffer.size() >= _buffer.capacity();
    }

private:
    static bool processItem(uint32_t item);

    static CircularBuffer<uint32_t, DISPATCHER_BUFFER_SIZE> _buffer;
    static IntervalTimer _timer;
};

#endif
