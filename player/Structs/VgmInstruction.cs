// SPDX-License-Identifier: GPL-3.0
using System;
using System.Diagnostics;

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
            if (!IsValidFmRegister(port, address))
            {
                Debug.WriteLine(
                    "Ignored register {0:X2} port {1} value {2:X2}",
                    address,
                    port,
                    value
                );

                return Nop();
            }

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

        private static bool IsValidFmRegister(byte port, byte address)
        {
            static bool IsOperator(byte address, byte offset)
            {
                var addr = address & 0x0f;

                return (address & 0xf0) == offset && (
                    (addr >= 0x00 && addr <= 0x02) ||
                    (addr >= 0x04 && addr <= 0x06) ||
                    (addr >= 0x08 && addr <= 0x0a) ||
                    (addr >= 0x0c && addr <= 0x0e)
                );
            }

            return (port == 0 && (
                    // global registers

                    // low frequency oscillator
                    address == 0x22 ||
                    // timer A frequency
                    address == 0x24 ||
                    address == 0x25 ||
                    // timer B frequency
                    address == 0x26 ||
                    // channel 3 mode and timer control
                    address == 0x27 ||
                    // key-on and key-off
                    address == 0x28 ||
                    // DAC output
                    address == 0x2a ||
                    // DAC enable
                    address == 0x2b
                )) ||
                // per-channel and per-operator registers

                // MUL (multiply) and DT (detune)
                IsOperator(address, 0x30) ||
                // TL (total level)
                IsOperator(address, 0x40) ||
                // AR (attack rate) and RS (rate scaling)
                IsOperator(address, 0x50) ||
                // DR (decay rate) and AM enable
                IsOperator(address, 0x60) ||
                // SR (sustain rate)
                IsOperator(address, 0x70) ||
                // RR (release rate) and SL (sustain level)
                IsOperator(address, 0x80) ||
                // SSG-EG
                IsOperator(address, 0x90) ||
                // frequency
                IsOperator(address, 0xa0) ||
                // algorithm and feedback
                (address >= 0xb0 && address <= 0xb2) ||
                // panning, PMS, AMS
                (address >= 0xb4 && address <= 0xb6);
        }
    }
}
