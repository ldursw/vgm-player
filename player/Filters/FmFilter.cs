using System;
using VgmPlayer.Structs;

namespace VgmPlayer.Filters
{
    class FmFilter
    {
        private struct FmChannel
        {
            public bool? Operator1;
            public bool? Operator2;
            public bool? Operator3;
            public bool? Operator4;

            public void Reset()
            {
                Operator1 = null;
                Operator2 = null;
                Operator3 = null;
                Operator4 = null;
            }
        }

        private static readonly byte?[] _fmMap = new byte?[0x200];
        private static readonly FmChannel[] _fmKeys = new FmChannel[6];

        public static Span<VgmInstruction> FilterCommand(VgmInstruction instr,
            Span<VgmInstruction> buffer)
        {
            if (instr.Type == InstructionType.End ||
                instr.Type == InstructionType.ResetImmediate)
            {
                Array.Fill(_fmMap, null);
                for (var i = 0; i < _fmKeys.Length; i++)
                {
                    _fmKeys[i].Reset();
                }

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

                if (!ProcessKeyCommand(port, address, value))
                {
                    return Span<VgmInstruction>.Empty;
                }

                if (port == 0 && address == 0x27)
                {
                    // filter timer data because they
                    // are not used on the genesis.
                    value &= 0b11000000;
                    if (value == 0b10000000 || value == 0b11000000)
                    {
                        // illegal value
                        return Span<VgmInstruction>.Empty;
                    }
                }

                if (_fmMap[(port << 8) | address] == value)
                {
                    var allowDuplicate =
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
                    // address == 0x24 ||
                    // address == 0x25 ||
                    // timer B frequency
                    // address == 0x26 ||
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

        private static bool ProcessKeyCommand(byte port, byte address, byte value)
        {
            if (port != 0 || address != 0x28)
            {
                return true;
            }

            var channel = (value & 0b00000111) switch
            {
                0b000 => 0,
                0b001 => 1,
                0b010 => 2,
                0b100 => 3,
                0b101 => 4,
                0b110 => 5,
                _ => -1,
            };
            if (channel == -1)
            {
                return false;
            }

            var op1 = (value & 0b00010000) != 0;
            var op2 = (value & 0b00100000) != 0;
            var op3 = (value & 0b01000000) != 0;
            var op4 = (value & 0b10000000) != 0;

            ref var ch = ref _fmKeys[channel];

            // If the key is pressed we allow the command to be repeated,
            // otherwise we filter the off commands on already off operators.
            var allowCommand =
                ch.Operator1 == null || (ch.Operator1.Value | op1) ||
                ch.Operator2 == null || (ch.Operator2.Value | op2) ||
                ch.Operator3 == null || (ch.Operator3.Value | op3) ||
                ch.Operator4 == null || (ch.Operator4.Value | op4);

            ch.Operator1 = op1;
            ch.Operator2 = op2;
            ch.Operator3 = op3;
            ch.Operator4 = op4;

            return allowCommand;
        }
    }
}
