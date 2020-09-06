// SPDX-License-Identifier: GPL-3.0
#ifndef INC_BUFFER
#define INC_BUFFER

// https://www.snellman.net/blog/archive/2016-12-13-ring-buffers/
template <class T, size_t N>
class CircularBuffer
{
public:
    // https://stackoverflow.com/a/10585550
    static_assert((N > 0) & !(N & (N - 1)), "Buffer size must be a power of two");

    void put(T item)
    {
        _buf[mask(_tail++)] = item;
    }

    bool get(T *value)
    {
        if (empty())
        {
            return false;
        }

        *value = _buf[mask(_head++)];

        return true;
    }

    void reset(void)
    {
        _head = 0;
        _tail = 0;
    }

    bool empty(void) const
    {
        return _head == _tail;
    }

    bool full(void) const
    {
        return size() == N;
    }

    constexpr size_t capacity(void) const
    {
        return N;
    }

    size_t size(void) const
    {
        return _tail - _head;
    }

private:
    T _buf[N];
    size_t _head = 0;
    size_t _tail = 0;

    constexpr const size_t mask(size_t val) const
    {
        return val & (N - 1);
    }
};

#endif
