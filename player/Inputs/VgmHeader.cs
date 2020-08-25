using System.IO;

namespace VgmReader.Inputs
{
    class VgmHeader
    {
        public uint Header { get; }
        public uint EofOffset { get; }
        public uint VersionNumber { get; }
        public uint Sn76489Clock { get; }
        public uint Ym2413Clock { get; }
        public uint Gd3Offset { get; }
        public uint TotalSamples { get; }
        public uint LoopOffset { get; }
        public uint LoopSamples { get; }
        public uint Rate { get; }
        public ushort Sn76489Feedback { get; }
        public byte Sn76489ShiftWidth { get; }
        public byte Sn76489Flags { get; }
        public uint Ym2612Clock { get; }
        public uint Ym2151Clock { get; }
        public uint DataOffset { get; }

        public VgmHeader(BinaryReader reader)
        {
            Header = reader.ReadUInt32();
            EofOffset = reader.ReadUInt32();
            VersionNumber = reader.ReadUInt32();
            Sn76489Clock = reader.ReadUInt32();
            Ym2413Clock = reader.ReadUInt32();
            Gd3Offset = CalculateOffset(reader.ReadUInt32(), 0x14);
            TotalSamples = reader.ReadUInt32();
            LoopOffset = CalculateOffset(reader.ReadUInt32(), 0x1c);
            LoopSamples = reader.ReadUInt32();
            Rate = reader.ReadUInt32();
            Sn76489Feedback = reader.ReadUInt16();
            Sn76489ShiftWidth = reader.ReadByte();
            Sn76489Flags = reader.ReadByte();
            Ym2612Clock = reader.ReadUInt32();
            Ym2151Clock = reader.ReadUInt32();
            DataOffset = CalculateDataOffset(reader.ReadUInt32());
        }

        private uint CalculateOffset(uint value, uint offset)
        {
            return value == 0 ? 0 : value + offset;
        }

        private uint CalculateDataOffset(uint value)
        {
            return VersionNumber < 0x150 ? 0x40 : value + 0x34;
        }
    }
}
