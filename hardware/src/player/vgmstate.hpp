#ifndef INC_VGMSTATE
#define INC_VGMSTATE

#ifdef ENABLE_PLAYER
#include "filebuffer.hpp"
#endif

#include <cstdint>

class VgmState
{
public:
    static int32_t waitSamples;
#ifdef ENABLE_PLAYER
    static FileBuffer pcmBank;
#endif
};

#endif
