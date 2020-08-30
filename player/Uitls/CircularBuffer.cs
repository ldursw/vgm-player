// SPDX-License-Identifier: GPL-3.0
using System.Threading;

namespace VgmReader.Utils
{
    class CircularBuffer<T>
    {
        public bool Full { get; private set; } = false;
        public bool Empty => !Full && (_head == _tail);
        public int Capacity => _buf.Length;
        public int Size => (_head >= _tail ? 0 : Capacity) + _head - _tail;

        private readonly SemaphoreSlim _inputSema;
        private readonly SemaphoreSlim _outputSema;
        private readonly T[] _buf;
        private int _head = 0;
        private int _tail = 0;

        public CircularBuffer(int capacity)
        {
            _inputSema = new SemaphoreSlim(capacity);
            _outputSema = new SemaphoreSlim(0);
            _buf = new T[capacity];
        }

        public void Add(T item)
        {
            _inputSema.Wait();

            _buf[_head] = item;

            if (Full)
            {
                _tail = (_tail + 1) % Capacity;
            }

            _head = (_head + 1) % Capacity;

            Full = _head == _tail;

            _outputSema.Release();
        }

        public T Take()
        {
            _outputSema.Wait();

            var value = _buf[_tail];
            _tail = (_tail + 1) % Capacity;
            Full = false;

            _inputSema.Release();

            return value;
        }

        public void Reset()
        {
            _head = _tail;
            Full = false;
        }
    }
}
