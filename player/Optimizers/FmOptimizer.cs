namespace VgmReader.Optimizers
{
    class FmOptimizer
    {
        private readonly byte[] _map = new byte[0x1ff];
        private byte _lastSample;

        public bool Write(byte port, byte address, byte value)
        {
            // TODO check more cases

            if (port == 0 && address == 0x27)
            {
                if (_map[0x27] == value)
                {
                    return false;
                }

                _map[0x27] = value;

                return true;
            }

            if (port == 0 && address == 0x2a)
            {
                var isEqual = _lastSample == value;

                _lastSample = value;

                return !isEqual;
            }

            return true;
        }
    }
}
