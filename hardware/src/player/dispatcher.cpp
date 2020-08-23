#include "dispatcher.hpp"
#include "chip/sn76489.hpp"
#include "chip/ym2612.hpp"
#include "clock/clock.hpp"
#include "player/vgmplayer.hpp"
#include "player/vgmstate.hpp"
#include "player/vgmcommands.hpp"

CircularBuffer<uint32_t, DISPATCHER_BUFFER_SIZE> Dispatcher::_buffer;
IntervalTimer Dispatcher::_timer;

void Dispatcher::enqueue(uint8_t command, uint8_t data1, uint8_t data2)
{
    _buffer.put(
        (command << 16) |
        (data1 << 8) |
        data2
    );
}

void Dispatcher::processImmediate(uint8_t command, uint8_t data1, uint8_t data2)
{
    switch (command)
    {
        case 0x01: // reset
            _buffer.reset();
            Sn76489::setup();
            Ym2612::setup();
            break;
    }
}

void Dispatcher::setup(void)
{
    ChipClock::setup();
    Sn76489::setup();
    Ym2612::setup();

    // Start timer with 44.1 kHz frequency.
    // That gives us ~22.675737us per interrupt
    // or ~4081 cycles at 180 MHz.
#if ENABLE_STREAM
    _timer.begin(process, (1.0 / 44100.0) * 1000000UL);
#elif ENABLE_PLAYER
    _timer.begin(VgmPlayer::play, (1.0 / 44100.0) * 1000000UL);
#endif
}

void Dispatcher::process(void)
{
    while (!_buffer.empty())
    {
        if (VgmState::waitSamples > 0)
        {
            VgmState::waitSamples--;

            break;
        }

        uint32_t item;
        if (!_buffer.get(&item) || !processItem(item))
        {
            break;
        }
    }

    Sn76489::update();
}

bool Dispatcher::processItem(uint32_t item)
{
    uint8_t command = (item >> 16) & 0xff;
    uint8_t data1 = (item >> 8) & 0xff;
    uint8_t data2 = item & 0xff;

    switch (command)
    {
        case 0:
        case 1: // fm write
        {
            WriteFmCommand::process(command, data1, data2);

            return false;
        }
        case 2: // psg write
        {
            WritePsgCommand::process(data1);

            return false;
        }
        case 3: // wait
        {
            if (!WaitCommand::process((data1 << 8) | data2))
            {
                return false;
            }

            break;
        }
        case 4: // fm write pcm
        {
            WriteFmCommand::process(0, 0x2a, data1);

            if (!WaitCommand::process(data2))
            {
                return false;
            }

            break;
        }
        case 5: // reset chip
            Sn76489::setup();
            Ym2612::setup();
            break;
        default:
        {
            while (true)
            {
                digitalWriteFast(13, HIGH);
                delay(200);
                digitalWriteFast(13, LOW);
                delay(200);
            }

            return false;
        }
    }

    return true;
}
