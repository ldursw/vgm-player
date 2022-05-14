using System;
using VgmPlayer.Structs;

namespace VgmPlayer.Filters
{
    class FmFilter
    {
        private static readonly byte?[] _fmMap = new byte?[0x200];

        public static Span<VgmInstruction> FilterCommand(VgmInstruction instr,
            Span<VgmInstruction> buffer)
        {
            if (instr.Type == InstructionType.End ||
                instr.Type == InstructionType.ResetImmediate)
            {
                Array.Fill(_fmMap, null);

                return Span<VgmInstruction>.Empty;
            }
            else if (instr.Type == InstructionType.FmWrite0 ||
                instr.Type == InstructionType.FmWrite1 ||
                instr.Type == InstructionType.FmSample)
            {
                byte port, address, value;
                if (instr.Type == InstructionType.FmSample)
                {
                    port = 0;
                    address = 0x2a;
                    value = instr.Data1;
                }
                else
                {
                    port = (byte)(instr.Type == InstructionType.FmWrite1 ? 1 : 0);
                    address = instr.Data1;
                    value = instr.Data2;
                }

                if (!IsValidFmRegister(port, address))
                {
                    return Span<VgmInstruction>.Empty;
                }

                if (_fmMap[(port << 8) | address] == value)
                {
                    var allowDuplicate =
                        // key on-off
                        (port == 0 && address == 0x28) ||
                        // frequency
                        (address == 0xa0 || address == 0xa1 || address == 0xa2) ||
                        (address == 0xa4 || address == 0xa5 || address == 0xa6) ||
                        (address == 0xa8 || address == 0xa9 || address == 0xaa) ||
                        (address == 0xac || address == 0xad || address == 0xae);

                    // possible duplicate value
                    if (!allowDuplicate)
                    {
                        if (instr.Type == InstructionType.FmSample)
                        {
                            buffer[0] = VgmInstruction.WaitSample(instr.Data2);

                            return buffer[0..1];
                        }
                        else
                        {
                            return Span<VgmInstruction>.Empty;
                        }
                    }
                }

                _fmMap[(port << 8) | address] = value;

                if (instr.Type == InstructionType.FmSample)
                {
                    buffer[0] = VgmInstruction.FmWriteSample(instr.Data1, instr.Data2);
                }
                else
                {
                    buffer[0] = VgmInstruction.FmWrite(port, address, value);
                }

                return buffer[0..1];
            }
            else
            {
                return Span<VgmInstruction>.Empty;
            }
        }

        public static bool IsValidFmRegister(byte port, byte address)
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
