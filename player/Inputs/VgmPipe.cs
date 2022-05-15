// SPDX-License-Identifier: GPL-3.0
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using VgmPlayer.Structs;
using VgmPlayer.Utils;

namespace VgmPlayer.Inputs
{
    class VgmPipe : IVgmInput
    {
        public IEnumerable<VgmInstruction> Instructions => GetInstructions();

        private const int InstructionsPerFrame = 735;
        private readonly Thread _readThread;
        private readonly CircularBuffer<VgmInstruction> _queue;

        public VgmPipe()
        {
            var pipeFunc = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                (Action)ReadPipeWindows : ReadPipeLinux;

            _queue = new CircularBuffer<VgmInstruction>(InstructionsPerFrame * 2);
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
            Span<byte> buffer = stackalloc byte[3];

            while (true)
            {
                _queue.Add(VgmInstruction.ResetImmediate());

                using var server = CreateServer();
                server.WaitForConnection();

                while (true)
                {
                    if (server.Read(buffer) != 3)
                    {
                        break;
                    }

                    ProcessCommand((InstructionType)buffer[0], buffer[1], buffer[2]);
                }
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
                _queue.Add(VgmInstruction.ResetImmediate());

                using var sock = socket.Accept();
                using var ns = new NetworkStream(sock);

                while (true)
                {
                    if (ns.Read(buffer, 0, 3) != 3)
                    {
                        break;
                    }

                    ProcessCommand((InstructionType)buffer[0], buffer[1], buffer[2]);
                }
            }
        }

        private void ProcessCommand(InstructionType instr, byte arg0, byte arg1)
        {
            if (instr == InstructionType.WaitSample)
            {
                // expand the wait instruction to keep
                // the latency constant and fix freezes
                // when PCM samples start or high latency
                // when there are few commands.
                var samples = arg0 | (arg1 << 8);
                for (var i = 0; i < samples; i++)
                {
                    _queue.Add(VgmInstruction.WaitSample(1));
                }
            }
            else
            {
                _queue.Add(new VgmInstruction(instr, arg0, arg1));
            }
        }
    }
}
