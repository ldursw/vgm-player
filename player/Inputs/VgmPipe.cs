using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using VgmPlayer.Structs;
using VgmReader.Utils;

namespace VgmReader.Inputs
{
    class VgmPipe : IVgmInput
    {
        public IEnumerable<VgmInstruction> Instructions => GetInstructions();

        private readonly Thread _readThread;
        private readonly CircularBuffer<VgmInstruction> _queue;

        public VgmPipe()
        {
            var pipeFunc = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                (Action)ReadPipeWindows : ReadPipeLinux;

            _queue = new CircularBuffer<VgmInstruction>(735 * 3);
            _readThread = new Thread(() => pipeFunc()) { IsBackground = true };
            _readThread.Start();
        }

        public void Dispose()
        {
        }

        private IEnumerable<VgmInstruction> GetInstructions()
        {
            while (true)
            {
                yield return _queue.Take();
            }
        }

        private NamedPipeServerStream CreateServer()
        {
            return new NamedPipeServerStream(
                "vgmstream",
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.None,
                9,
                9
            );
        }

        private void ReadPipeWindows()
        {
            byte[] buffer = new byte[3];

            while (true)
            {
                using var server = CreateServer();
                server.WaitForConnection();

                while (true)
                {
                    if (server.Read(buffer, 0, 3) != 3)
                    {
                        break;
                    }

                    _queue.Add(new VgmInstruction(
                        (InstructionType)buffer[0],
                        buffer[1],
                        buffer[2]
                    ));
                }

                server.Dispose();
            }
        }

        private void ReadPipeLinux()
        {
            byte[] buffer = new byte[3];

            File.Delete("/tmp/vgmstream.sock");
            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            socket.Bind(new UnixDomainSocketEndPoint("/tmp/vgmstream.sock"));
            socket.Listen(1);

            while (true)
            {
                var sock = socket.Accept();
                var ns = new NetworkStream(sock);

                while (true)
                {
                    if (ns.Read(buffer, 0, 3) != 3)
                    {
                        break;
                    }

                    _queue.Add(new VgmInstruction(
                        (InstructionType)buffer[0],
                        buffer[1],
                        buffer[2]
                    ));
                }

                sock.Dispose();
            }
        }
    }
}
