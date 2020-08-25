using System;

namespace VgmPlayer.Structs
{
    enum InstructionType : byte
    {
        Nop = 0x00,
        PsgWrite = 0x01,
        FmWrite0 = 0x02,
        FmWrite1 = 0x03,
        WaitSample = 0x04,
        End = 0x05,
        FmSample = 0x06,

        ResetImmediate = 0x81,
    }

    struct VgmInstruction
    {
        public readonly InstructionType Type;
        public readonly byte Data1;
        public readonly byte Data2;

        public VgmInstruction(InstructionType type, byte arg1, byte arg2)
        {
            Type = type;
            Data1 = arg1;
            Data2 = arg2;
        }

        public static VgmInstruction Nop()
        {
            return new VgmInstruction(InstructionType.Nop, 0, 0);
        }

        public static VgmInstruction PsgWrite(byte data)
        {
            return new VgmInstruction(InstructionType.PsgWrite, data, 0);
        }

        public static VgmInstruction FmWrite(byte port, byte address, byte value)
        {
            var type = port switch
            {
                0 => InstructionType.FmWrite0,
                1 => InstructionType.FmWrite1,
                _ => throw new ArgumentException()
            };

            return new VgmInstruction(type, address, value);
        }

        public static VgmInstruction WaitSample(ushort value)
        {
            return WaitSample((byte)(value >> 8), (byte)(value & 0xff));
        }

        public static VgmInstruction WaitSample(byte high, byte low)
        {
            return new VgmInstruction(InstructionType.WaitSample, low, high);
        }

        public static VgmInstruction End()
        {
            return new VgmInstruction(InstructionType.End, 0, 0);
        }

        public static VgmInstruction FmWriteSample(byte sample, byte wait)
        {
            return new VgmInstruction(InstructionType.FmSample, sample, wait);
        }

        public static VgmInstruction ResetImmediate()
        {
            return new VgmInstruction(InstructionType.ResetImmediate, 0, 0);
        }
    }
}
