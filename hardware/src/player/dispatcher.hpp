#ifndef INC_PLAYER_DISPATCHER
#define INC_PLAYER_DISPATCHER

#include <cstdint>
#include <Arduino.h>
#include "../util/buffer.hpp"

class InstructionType
{
public:
    static const uint8_t Nop = 0x00;
    static const uint8_t PsgWrite = 0x01;
    static const uint8_t FmWrite0 = 0x02;
    static const uint8_t FmWrite1 = 0x03;
    static const uint8_t WaitSample = 0x04;
    static const uint8_t End = 0x05;
    static const uint8_t FmSample = 0x06;

    static const uint8_t ResetImmediate = 0x81;
};

class __attribute__((packed, aligned(4))) Instruction
{
public:
    constexpr uint8_t type() const { return _type; }
    constexpr uint8_t data1() const { return _u8[0]; }
    constexpr uint8_t data2() const { return _u8[1]; }
    constexpr uint16_t data() const { return _u16; }

    constexpr Instruction() : _u16(0), _type(InstructionType::Nop)
    {
    }

    constexpr Instruction(uint8_t command, uint8_t data1, uint8_t data2) :
        _u8{data1, data2}, _type(command)
    {
    }

private:
    union
    {
        uint16_t _u16;
        uint8_t _u8[2];
    };
    uint8_t _type;
};

class Dispatcher {
public:
    static void enqueue(Instruction);
    static void setup(void);
    static void process(void);

    static bool isBufferFull(void)
    {
        return _buffer.size() >= _buffer.capacity();
    }

private:
    static bool processItem(Instruction item);

    static constexpr size_t BufferSize = 16;
    static CircularBuffer<Instruction, BufferSize> _buffer;
};

#endif
