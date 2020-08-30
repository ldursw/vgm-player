// SPDX-License-Identifier: GPL-3.0
#ifndef INC_VGMPLAYER
#define INC_VGMPLAYER

#include "vgmfile.hpp"
#include <stdint.h>

class VgmPlayer
{
public:
    static bool setup(const char *filename);
    static void play(void);
};

#endif
