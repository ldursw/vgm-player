#include "vgmstate.hpp"

int32_t VgmState::waitSamples = 0;

#ifdef ENABLE_PLAYER
FileBuffer VgmState::pcmBank;
#endif
