using System;
using System.Collections.Generic;
using System.IO;
using VgmReader.Inputs;

namespace VgmReader
{
    class VgmData
    {
        public IEnumerable<uint> Instructions { get; }

        private readonly BinaryReader _reader;
        private long _dataBankOffset;
        private uint _dataBankSize;
        private uint _dataBankIndex;

        public VgmData(VgmHeader header, BinaryReader reader)
        {
            reader.BaseStream.Seek(header.DataOffset, SeekOrigin.Begin);

            _reader = reader;

            Instructions = GetInstructions(header);
        }

        private IEnumerable<uint> GetInstructions(VgmHeader header)
        {
            yield return WaitSample(0x7fff);

            while (true)
            {
                var value = _reader.ReadByte();
                if (value == 0x66)
                {
                    if (header.LoopSamples > 0)
                    {
                        _reader.BaseStream.Position = header.LoopOffset;
                        continue;
                    }
                    else
                    {
                        yield return End();
                        break;
                    }
                }

                if (value >= 0x30 && value <= 0x3f || value == 0x4f)
                {
                    _reader.BaseStream.Position++;

                    continue;
                }

                if (value >= 0x40 && value <= 0x4e)
                {
                    _reader.BaseStream.Position++;
                    if (header.VersionNumber >= 0x160)
                    {
                        _reader.BaseStream.Position++;
                    }

                    continue;
                }

                if ((value >= 0xa1 && value <= 0xaf) ||
                    (value >= 0xbc && value <= 0xbf) ||
                    value == 0xb1)
                {
                    _reader.BaseStream.Position += 2;

                    continue;
                }

                if ((value >= 0xc5 && value <= 0xcf) ||
                    (value >= 0xd5 && value <= 0xdf))
                {
                    _reader.BaseStream.Position += 3;

                    continue;
                }

                if (value >= 0xe1 && value <= 0xff)
                {
                    _reader.BaseStream.Position += 4;

                    continue;
                }

                uint? instr = value switch
                {
                    0x50 => PsgWrite(_reader.ReadByte()),
                    0x52 => FmWrite(0, _reader.ReadByte(), _reader.ReadByte()),
                    0x53 => FmWrite(1, _reader.ReadByte(), _reader.ReadByte()),
                    0x61 => WaitSample(_reader.ReadUInt16()),
                    0x62 => WaitSample(0x2df),
                    0x63 => WaitSample(0x372),
                    _ when value >= 0x70 && value <= 0x7f =>
                        WaitSample((ushort)(value - 0x70)),
                    _ when value >= 0x80 && value <= 0x8f =>
                        FmWriteWait((byte)(value - 0x80)),
                    0x67 => SetDataBank(),
                    0x68 => PcmRamWrite(),
                    0xb2 => PwmWrite(_reader.ReadByte(), _reader.ReadByte()),
                    0xe0 => SeekDataBank(_reader.ReadUInt32()),
                    _ => null
                };

                if (instr == null)
                {
                    var pos = _reader.BaseStream.Position;

                    throw new Exception($"Unknown instruction {value:X2} at {pos}");
                }

                if (instr == 0)
                {
                    continue;
                }

                yield return instr.Value;
            }
        }

        private uint PsgWrite(byte data)
        {
            return VgmInstructions.PsgWrite(data);
        }

        private uint FmWrite(byte port, byte address, byte value)
        {
            return VgmInstructions.FmWrite(port, address, value);
        }

        private uint WaitSample(ushort value)
        {
            return VgmInstructions.WaitSample(value);
        }

        private uint End()
        {
            return VgmInstructions.End();
        }

        private uint FmWriteWait(byte wait)
        {
            var oldPos = _reader.BaseStream.Position;
            _reader.BaseStream.Position = _dataBankOffset + _dataBankIndex++;

            var sample = _reader.ReadByte();
            _reader.BaseStream.Position = oldPos;

            return VgmInstructions.FmWriteSample(sample, wait);
        }

        private uint SetDataBank()
        {
            _reader.ReadByte(); // skip 0x66
            _reader.ReadByte(); // type
            _dataBankSize = _reader.ReadUInt32();
            _dataBankOffset = _reader.BaseStream.Position;

            _reader.BaseStream.Position += _dataBankSize;

            return 0;
        }

        private uint SeekDataBank(uint offset)
        {
            _dataBankIndex = offset;

            return 0;
        }

        private uint PcmRamWrite()
        {
            _reader.BaseStream.Position += 11;

            return 0;
        }

        private uint PwmWrite(byte address, byte value)
        {
            return 0;
        }
    }
}
