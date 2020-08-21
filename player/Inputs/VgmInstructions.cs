namespace VgmReader.Inputs
{
    class VgmInstructions
    {
        public static uint PsgWrite(byte data)
        {
            return (uint)((0x01 << 16) | (data << 8));
        }

        public static uint FmWrite(byte port, byte address, byte value)
        {
            return (uint)(((0x02 + port) << 16) | (address << 8) | value);
        }

        public static uint WaitSample(ushort value)
        {
            return (uint)((0x04 << 16) | value);
        }

        public static uint WaitSample(byte high, byte low)
        {
            return (uint)((0x04 << 16) | (high << 8) | low);
        }

        public static uint End()
        {
            return 0x05 << 16;
        }

        public static uint FmWriteSample(byte sample, byte wait)
        {
            return (uint)((0x06 << 16) | (sample << 8) | wait);
        }
    }
}
