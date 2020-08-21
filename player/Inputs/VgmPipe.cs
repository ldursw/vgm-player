using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using VgmReader.Optimizers;

namespace VgmReader.Inputs
{
    class VgmPipe : IVgmInput
    {
        public IEnumerable<uint> Instructions => GetInstructions();

        private readonly Thread _readThread;
        private readonly CircularBuffer<Instruction> _queue;
        private readonly FmOptimizer _optimizer;

        public VgmPipe()
        {
            _optimizer = new FmOptimizer();
            _queue = new CircularBuffer<Instruction>(735 * 3);
            _readThread = new Thread(ReadPipe) { IsBackground = true };
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

        private void ReadPipe()
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

                    if ((buffer[0] == 0x02 || buffer[0] == 0x03) &&
                        !_optimizer.Write((byte)(buffer[0] - 0x02), buffer[1], buffer[2]))
                    {
                        continue;
                    }

                    _queue.Add(new Instruction(buffer[0], buffer[1], buffer[2]));
                }

                server.Dispose();
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
