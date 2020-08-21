#ifndef INC_BUFFER
#define INC_BUFFER

#include <cstdio>
#include <memory>

// https://github.com/embeddedartistry/embedded-resources/blob/master/examples/cpp/circular_buffer.cpp
template <class T, size_t N>
class CircularBuffer
{
public:
    void put(T item)
    {
        _buf[_head] = item;

        if (_full)
        {
            _tail = (_tail + 1) % N;
        }

        _head = (_head + 1) % N;

        _full = _head == _tail;
    }

    bool get(T *value)
    {
        if (empty())
        {
            return false;
        }

        //Read data and advance the tail (we now have a free space)
        *value = _buf[_tail];
        _full = false;
        _tail = (_tail + 1) % N;

        return true;
    }

    void reset(void)
    {
        _head = _tail;
        _full = false;
    }

    bool empty(void) const
    {
        //if head and tail are equal, we are empty
        return !_full && (_head == _tail);
    }

    bool full(void) const
    {
        //If tail is ahead the head by 1, we are full
        return _full;
    }

    size_t capacity(void) const
    {
        return N;
    }

    size_t size(void) const
    {
        if (_full)
        {
            return N;
        }

        return (_head >= _tail ? 0 : N) + _head - _tail;
    }

private:
    T _buf[N];
    size_t _head = 0;
    size_t _tail = 0;
    bool _full = false;
};

#endif
