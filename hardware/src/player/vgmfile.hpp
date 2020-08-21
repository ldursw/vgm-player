#ifndef INC_VGMFILE
#define INC_VGMFILE

#ifdef ENABLE_PLAYER

#include <cstdint>
#include "filebuffer.hpp"

struct VgmHeader
{
    uint32_t signature;
    uint32_t eofOffset;
    uint32_t version;
    uint32_t sn76489Clock;
    uint32_t ym2413Clock;
    uint32_t gd3Offset;
    uint32_t totalSamples;
    uint32_t loopOffset;
    uint32_t loopSamples;
    uint32_t rate;
    uint16_t sn76489Feedback;
    uint8_t sn76489ShiftWidth;
    uint8_t sn76489Flags;
    uint32_t ym2612Clock;
    uint32_t ym2151Clock;
    uint32_t dataOffset;
};

class VgmFile
{
public:
    static void setup(const char *filename);
    static bool process(void);

    static bool isValid(void)
    {
        return valid;
    }

private:
    static VgmHeader header;
    static bool valid;
    static FileBuffer fileBuffer;

    static uint8_t readByte(File &file);
    static uint16_t readShort(File &file);
    static uint32_t readInt(File &file);
    static void skip(File &file, int length);
};

#endif

#endif
