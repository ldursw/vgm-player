// SPDX-License-Identifier: GPL-3.0
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using VgmPlayer.Structs;

namespace VgmPlayer.Inputs
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
            _stream = new MemoryStream();

            using (var fs = File.OpenRead(filename))
            {
                Span<byte> buf = stackalloc byte[4];
                fs.Read(buf);
                fs.Position = 0;

                Span<byte> magicBytes = stackalloc byte[4]
                {
                    (byte)'V',
                    (byte)'g',
                    (byte)'m',
                    (byte)' '
                };
                if (magicBytes.SequenceCompareTo(buf) != 0)
                {
                    using var gz = new GZipStream(fs, CompressionMode.Decompress);
                    CopyStream(gz, _stream);
                }
                else
                {
                    _stream.SetLength(fs.Length);
                    CopyStream(fs, _stream);
                }
            }

            _stream.Position = 0;
            _binaryReader = new BinaryReader(_stream);

            Header = new VgmHeader(_binaryReader);
            Data = new VgmData(Header, _binaryReader);
        }

        public void Dispose()
        {
            _binaryReader.Dispose();
            _stream.Dispose();
        }

        private void CopyStream(Stream source, Stream destination)
        {
            int read;
            Span<byte> buf = stackalloc byte[0x100];
            while ((read = source.Read(buf)) > 0)
            {
                destination.Write(buf.Slice(0, read));
            }
        }
    }
}
