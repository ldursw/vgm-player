using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using VgmReader.Utils;

namespace VgmReader.Inputs
{
    class VgmPipe : IVgmInput
    {
        public IEnumerable<uint> Instructions => GetInstructions();

        private readonly Thread _readThread;
        private readonly CircularBuffer<Instruction> _queue;

        public VgmPipe()
        {
            var pipeFunc = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                (Action)ReadPipeWindows : ReadPipeLinux;

            _queue = new CircularBuffer<Instruction>(735 * 3);
            _readThread = new Thread(() => pipeFunc()) { IsBackground = true };
            _readThread.Start();
        }

        public void Dispose()
        {
        }

        private IEnumerable<uint> GetInstructions()
        {
            while (true)
            {
                var item = _queue.Take();
                var command = item.Command;
                var arg1 = item.Arg1;
                var arg2 = item.Arg2;

                switch (command)
                {
                    case InstructionType.WritePsg:
                        yield return VgmInstructions.PsgWrite(arg1);
                        break;
                    case InstructionType.WriteFmPcm:
                        yield return VgmInstructions.FmWriteSample(arg1, arg2);
                        break;
                    case InstructionType.WriteFmPort0:
                        yield return VgmInstructions.FmWrite(0, arg1, arg2);
                        break;
                    case InstructionType.WriteFmPort1:
                        yield return VgmInstructions.FmWrite(1, arg1, arg2);
                        break;
                    case InstructionType.Wait:
                        yield return VgmInstructions.WaitSample(arg1, arg2);
                        break;
                }
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

                    _queue.Add(new Instruction(buffer[0], buffer[1], buffer[2]));
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

                    _queue.Add(new Instruction(buffer[0], buffer[1], buffer[2]));
                }

                sock.Dispose();
            }
        }

        private struct Instruction
        {
            public readonly InstructionType Command;
            public readonly byte Arg1;
            public readonly byte Arg2;

            public Instruction(byte command, byte arg1, byte arg2)
            {
                Command = (InstructionType)command;
                Arg1 = arg1;
                Arg2 = arg2;
            }
        }

        private enum InstructionType : byte
        {
            WritePsg = 0x01,
            WriteFmPort0 = 0x02,
            WriteFmPort1 = 0x03,
            WriteFmPcm = 0x04,
            Wait = 0x05,
        }
    }
}
