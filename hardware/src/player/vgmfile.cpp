#ifdef ENABLE_PLAYER

#include <SdFat.hpp>
#include <cstdint>
#include <cstdlib>
#include "vgmfile.hpp"
#include "vgmcommands.hpp"
#include "vgmstate.hpp"
#include "util/hal.hpp"

static SdFatCls SD;
static File file;
static File pcmFile;

VgmHeader VgmFile::header = {};
bool VgmFile::valid = false;
FileBuffer VgmFile::fileBuffer;

uint8_t VgmFile::readByte(File &file)
{
    return fileBuffer.getByte();
}

uint16_t VgmFile::readShort(File &file)
{
    return readByte(file) | readByte(file) << 8;
}

uint32_t VgmFile::readInt(File &file)
{
    return readShort(file) | readShort(file) << 16;
}

void VgmFile::skip(File &file, int length)
{
    fileBuffer.skip(length);
}

void VgmFile::setup(const char *filename)
{
    valid = false;

    if (!SD.begin())
    {
        return;
    }

    file = SD.open(filename, FILE_READ);
    if (!file)
    {
        return;
    }

    file.readBytes((uint8_t *)&header, sizeof(VgmHeader));

    if (header.signature != 0x206d6756)
    {
        file.close();

        return;
    }

    uint32_t offset = header.version < 0x150 ? 0x40 : (header.dataOffset + 0x34);
    file.seekSet(offset);

    fileBuffer.setup(file, offset, header.eofOffset - 4);

    pcmFile = SD.open(filename, FILE_READ);

    valid = true;
}

bool VgmFile::process(void)
{
    while (true)
    {
        if (VgmState::waitSamples > 0)
        {
            VgmState::waitSamples--;

            return false;
        }

        if (fileBuffer.isEof())
        {
            if (header.loopOffset == 0)
            {
                return true;
            }

            uint32_t offset = header.version < 0x150 ? 0x40 : (header.dataOffset + 0x34);
            fileBuffer.setIndex(header.loopOffset + 0x1c - offset);

            return false;
        }

        uint8_t value = readByte(file);
        if (value == 0x66)
        {
            if (header.loopOffset == 0)
            {
                return true;
            }

            uint32_t offset = header.version < 0x150 ? 0x40 : (header.dataOffset + 0x34);
            fileBuffer.setIndex(header.loopOffset + 0x1c - offset);

            return false;
        }

        if (value >= 0x40 && value <= 0x4e && header.version >= 0x160)
        {
            skip(file, 2);

            continue;
        }

        if (value >= 0x30 && value <= 0x4f)
        {
            skip(file, 1);

            continue;
        }

        if (value >= 0xa0 && value <= 0xbf)
        {
            skip(file, 2);

            continue;
        }

        if (value >= 0xc0 && value <= 0xdf)
        {
            skip(file, 3);

            continue;
        }

        if (value >= 0xe1)
        {
            skip(file, 4);

            continue;
        }

        switch (value)
        {
            case 0x50: // psg write
            {
                if (!WritePsgCommand::process(readByte(file)))
                {
                    return false;
                }

                break;
            }
            case 0x52:
            case 0x53: // fm write
            {
                uint8_t port = value - 0x52;
                uint8_t addr = readByte(file);
                uint8_t value = readByte(file);
                if (!WriteFmCommand::process(port, addr, value))
                {
                    return false;
                }

                break;
            }
            case 0x61: // wait
            {
                if (!WaitCommand::process(readShort(file)))
                {
                    return false;
                }

                break;
            }
            case 0x62: // wait 60hz
            {
                if (!WaitCommand::process(0x2df))
                {
                    return false;
                }

                break;
            }
            case 0x63: // wait 50hz
            {
                if (!WaitCommand::process(0x372))
                {
                    return false;
                }

                break;
            }
            case 0x70:
            case 0x71:
            case 0x72:
            case 0x73:
            case 0x74:
            case 0x75:
            case 0x76:
            case 0x77:
            case 0x78:
            case 0x79:
            case 0x7a:
            case 0x7b:
            case 0x7c:
            case 0x7d:
            case 0x7e:
            case 0x7f: // wait
            {
                if (!WaitCommand::process(value - 0x70))
                {
                    return false;
                }

                break;
            }
            case 0x80:
            case 0x81:
            case 0x82:
            case 0x83:
            case 0x84:
            case 0x85:
            case 0x86:
            case 0x87:
            case 0x88:
            case 0x89:
            case 0x8a:
            case 0x8b:
            case 0x8c:
            case 0x8d:
            case 0x8e:
            case 0x8f: // fm write wait
            {
                if (!WriteFmPcmCommand::process(value - 0x80))
                {
                    return false;
                }

                break;
            }
            case 0x67: // data block
            {
                readByte(file);                // skip 0x66
                uint8_t type = readByte(file); // type
                uint32_t size = readInt(file); // size
                uint32_t offset = fileBuffer.absolutePosition();

                skip(file, size);

                // YM2612 PCM data
                if (type != 0x00)
                {
                    return false;
                }

                if (!SetDataBankCommand::process(pcmFile, offset, size))
                {
                    return false;
                }

                break;
            }
            case 0x68: // ram write
            {
                skip(file, 11);
                break;
            }
            case 0xe0: // seek data bank
            {
                if (!SetPcmOffsetCommand::process(readInt(file)))
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

                return true;
            }
        }
    }

    return false;
}

#endif
