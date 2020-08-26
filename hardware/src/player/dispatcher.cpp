#include "dispatcher.hpp"
#include "chip/sn76489.hpp"
#include "chip/ym2612.hpp"
#include "clock/clock.hpp"
#include "player/vgmplayer.hpp"
#include "player/vgmstate.hpp"
#include "player/vgmcommands.hpp"

CircularBuffer<Instruction, DISPATCHER_BUFFER_SIZE> Dispatcher::_buffer;
IntervalTimer Dispatcher::_timer;

void Dispatcher::enqueue(Instruction instruction)
{
    switch (instruction.type())
    {
        case InstructionType::ResetImmediate:
            _buffer.reset();
            Sn76489::setup();
            Ym2612::setup();
            break;
        default:
            _buffer.put(instruction);
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
    _timer.begin(process, (1.0 / 44100.0) * 1'000'000UL);
}

void Dispatcher::process(void)
{
#if ENABLE_PLAYER
    VgmPlayer::play();
#elif ENABLE_STREAM
    while (!_buffer.empty())
    {
        if (VgmState::waitSamples > 0)
        {
            VgmState::waitSamples--;

            break;
        }

        Instruction item;
        if (!_buffer.get(&item) || !processItem(item))
        {
            break;
        }
    }
#endif

    Sn76489::update();
}

bool Dispatcher::processItem(Instruction item)
{
    switch (item.type())
    {
        case InstructionType::Nop:
        {
            return true;
        }
        case InstructionType::PsgWrite:
        {
            WritePsgCommand::process(item.data1());

            return false;
        }
        case InstructionType::FmWrite0:
        {
            WriteFmCommand::process(0, item.data1(), item.data2());

            return false;
        }
        case InstructionType::FmWrite1:
        {
            WriteFmCommand::process(1, item.data1(), item.data2());

            return false;
        }
        case InstructionType::WaitSample:
        {
            if (!WaitCommand::process(item.data()))
            {
                return false;
            }

            break;
        }
        case InstructionType::End:
        {
            Sn76489::setup();
            Ym2612::setup();
            break;
        }
        case InstructionType::FmSample:
        {
            WriteFmCommand::process(0, 0x2a, item.data1());

            if (!WaitCommand::process(item.data2()))
            {
                return false;
            }

            break;
        }
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
