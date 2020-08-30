// SPDX-License-Identifier: GPL-3.0
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using VgmPlayer.Structs;

namespace VgmReader.Inputs
{
    class VgmFile : IVgmInput
    {
        public VgmHeader Header { get; }
        public VgmData Data { get; }
        public IEnumerable<VgmInstruction> Instructions => Data.Instructions;

        private readonly Stream _stream;
        private readonly BinaryReader _binaryReader;

        public VgmFile(string filename)
        {
            _stream = new FileStream(filename, FileMode.Open);
            _binaryReader = new BinaryReader(_stream);

            var header = _binaryReader.ReadUInt32();
            _stream.Seek(0, SeekOrigin.Begin);

            if (header != 0x206d6756)
            {
                var ms = new MemoryStream();
                using (var gz = new GZipStream(_stream, CompressionMode.Decompress))
                {
                    gz.CopyTo(ms);
                    ms.Position = 0;
                }

                _stream.Dispose();
                _stream = ms;
                _binaryReader = new BinaryReader(ms);
            }

            Header = new VgmHeader(_binaryReader);
            Data = new VgmData(Header, _binaryReader);
        }

        public void Dispose()
        {
            _stream.Dispose();
            _binaryReader.Dispose();
        }
    }
}
