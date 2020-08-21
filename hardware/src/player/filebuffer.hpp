#ifndef INC_FILEBUFFER
#define INC_FILEBUFFER

#ifdef ENABLE_PLAYER

#define PCM_BUFFER 1536

#include <SdFat.h>
#include <cstdint>

class FileBuffer
{
public:
    void setup(File file, uint32_t base, uint32_t size)
    {
        _file = file;
        _base = base;
        _size = size;
        _page = 0;
        _index = 0;

        fillBuffer();
    }

    uint8_t getByte(void)
    {
        uint8_t sample = _buffer[_index++];

        if (_index == PCM_BUFFER)
        {
            _page++;
            _index = 0;
            fillBuffer();
        }

        return sample;
    }

    void setIndex(uint32_t index)
    {
        uint32_t newPage = index / PCM_BUFFER;
        if (newPage != _page)
        {
            _page = newPage;
            fillBuffer();
        }

        _index = index % PCM_BUFFER;
    }

    void skip(uint32_t size)
    {
        setIndex(position() + size);
    }

    uint32_t position(void)
    {
        return (_page * PCM_BUFFER) + _index;
    }

    uint32_t absolutePosition(void)
    {
        return _base + position();
    }

    bool isEof(void)
    {
        return position() >= _size;
    }

private:
    File _file;
    uint8_t _buffer[PCM_BUFFER];

    uint32_t _base;
    uint32_t _size;
    uint32_t _page;
    uint32_t _index;

    void fillBuffer(void)
    {
        uint32_t offset = (_page * PCM_BUFFER);
        _file.seekSet(_base + offset);

        uint32_t size = min((uint32_t)PCM_BUFFER, _size - offset);
        _file.read(_buffer, size);
    }
};

#endif

#endif
