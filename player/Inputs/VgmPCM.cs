using System.Collections.Generic;
using System.IO;

namespace VgmReader.Inputs
{
    class VgmPCM : IVgmInput
    {
        public IEnumerable<uint> Instructions => GetInstructions();

        private readonly FileStream _stream;

        public VgmPCM(string filename)
        {
            _stream = new FileStream(filename, FileMode.Open);
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        private IEnumerable<uint> GetInstructions()
        {
            yield return VgmInstructions.WaitSample(0x7fff);
            yield return VgmInstructions.FmWrite(0, 0x2b, 0x80);
            
            while (_stream.CanRead && _stream.Position < _stream.Length)
            {
                var sample = _stream.ReadByte();
                if (sample == -1)
                {
                    break;
                }

                yield return VgmInstructions.FmWriteSample((byte)sample, 1);
            }

            yield return VgmInstructions.FmWrite(0, 0x2b, 0x00);
            yield return VgmInstructions.End();
        }
    }
}
