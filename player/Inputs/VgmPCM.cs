// SPDX-License-Identifier: GPL-3.0
using System.Collections.Generic;
using System.IO;
using VgmPlayer.Structs;

namespace VgmReader.Inputs
{
    class VgmPCM : IVgmInput
    {
        public IEnumerable<VgmInstruction> Instructions => GetInstructions();

        private readonly FileStream _stream;

        public VgmPCM(string filename)
        {
            _stream = new FileStream(filename, FileMode.Open);
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        private IEnumerable<VgmInstruction> GetInstructions()
        {
            yield return VgmInstruction.WaitSample(0x7fff);
            yield return VgmInstruction.FmWrite(0, 0x2b, 0x80);
            yield return VgmInstruction.FmWrite(1, 0xb6, 0xc0);

            while (_stream.CanRead && _stream.Position < _stream.Length)
            {
                var sample = _stream.ReadByte();
                if (sample == -1)
                {
                    break;
                }

                yield return VgmInstruction.FmWriteSample((byte)sample, 1);
            }

            yield return VgmInstruction.FmWrite(0, 0x2b, 0x00);
            yield return VgmInstruction.End();
        }
    }
}
