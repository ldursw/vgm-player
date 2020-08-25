#ifdef ENABLE_PLAYER

#include <Arduino.h>
#include "vgmplayer.hpp"

bool VgmPlayer::setup(const char *filename)
{
    VgmFile::setup(filename);

    return VgmFile::isValid();
}

void VgmPlayer::play(void)
{
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
