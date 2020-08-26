#ifndef INC_VGMCOMMANDS
#define INC_VGMCOMMANDS

#include "../chip/sn76489.hpp"
#include "../chip/ym2612.hpp"
#include "vgmstate.hpp"
#include <cstdint>

class WaitCommand
{
public:
    static bool process(uint32_t samples)
    {
        VgmState::waitSamples += samples;

        return VgmState::waitSamples > 0;
    }
};

class WriteFmCommand
{
public:
    static bool process(uint8_t port, uint8_t address, uint8_t value)
    {
        Ym2612::pushData(port << 1, address);
        Ym2612::pushData((port << 1) | 1, value);

        return false;
    }
};

class WritePsgCommand
{
public:
    static bool process(uint8_t value)
    {
        Sn76489::pushData(value);

        return false;
    }
};

#ifdef ENABLE_PLAYER

class SetPcmOffsetCommand
{
public:
    static bool process(uint32_t offset)
    {
        VgmState::pcmBank.setIndex(offset);

        return true;
    }
};

class SetDataBankCommand
{
public:
    static bool process(File &file, uint32_t base, uint32_t size)
    {
        VgmState::pcmBank.setup(file, base, size);

        return true;
    }
};

class WriteFmPcmCommand
{
public:
    static bool process(int32_t samples)
    {
        uint8_t sample = VgmState::pcmBank.getByte();
        WriteFmCommand::process(0, 0x2a, sample);

        return WaitCommand::process(samples);
    }
};

#endif // ENABLE_PLAYER

#endif // INC_VGMCOMMANDS
