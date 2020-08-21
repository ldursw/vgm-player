#ifdef ENABLE_PLAYER

#include <Arduino.h>
#include "vgmplayer.hpp"
#include "chip/sn76489.hpp"

bool VgmPlayer::setup(const char *filename)
{
    VgmFile::setup(filename);

    return VgmFile::isValid();
}

void VgmPlayer::play(void)
{
    Sn76489::update();

    if (!VgmFile::isValid())
    {
        return;
    }

    if (VgmFile::process())
    {
        while (true);
    }
}

#endif
