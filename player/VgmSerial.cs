using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using VgmReader.Inputs;

namespace VgmReader
{
    class VgmSerial
    {
        private static Stream _serial;

        [DllImport("kernel32", EntryPoint = "CreateFileW", SetLastError = true,
            CharSet = CharSet.Unicode, BestFitMapping = false, ExactSpelling = true)]
        private static extern IntPtr CreateFile(string lpFileName, int dwDesiredAccess,
            int dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition,
            int dwFlagsAndAttributes, IntPtr hTemplateFile);

        public static void Play(string name, IVgmInput vgm)
        {
            var t = new Thread(() => Run(name, vgm))
            {
                IsBackground = true,
            };

            t.Start();
        }

        private static void Run(string name, IVgmInput vgm)
        {
            _serial = OpenSerial(name);

            ResetChipImmediate();

            foreach (var inst in vgm.Instructions)
            {
                var command = (byte)(inst >> 16);
                var data1 = (byte)(inst >> 8);
                var data2 = (byte)inst;

                if (command == 0x01)
                {
                    WritePsg(data1);
                }
                else if (command == 0x02 || command == 0x03)
                {
                    WriteFm((byte)(command - 0x02), data1, data2);
                }
                else if (command == 0x04 && data2 > 0)
                {
                    WaitSamples((ushort)(((data1 << 8) & 0xff00) | (data2 & 0xff)));
                }
                else if (command == 0x05)
                {
                    ResetChip();
                }
                else if (command == 0x06)
                {
                    WriteFmPcm(data1, data2);
                }
            }

            ResetChip();
            VgmCommandParser.Reset();
            vgm.Dispose();
            _serial?.Flush();
        }

        public static void Stop()
        {
            if (_serial != null)
            {
                var sp = _serial;
                _serial = null;

                sp.Flush();
                // reset chip immediate
                sp.WriteByte(0x81);
                sp.WriteByte(0x00);
                sp.WriteByte(0x00);
                sp.Flush();
                sp.Dispose();
            }
        }

        private static void WriteFm(byte port, byte address, byte value)
        {
            WriteCommand(port, address, value);
        }

        private static void WritePsg(byte data)
        {
            WriteCommand(0x02, data, 0x00);
        }

        private static void WaitSamples(ushort samples)
        {
            WriteCommand(
                0x03,
                (byte)(samples >> 8),
                (byte)(samples & 0xff)
            );
        }

        private static void ResetChip()
        {
            WriteCommand(0x05, 0x00, 0x00);
        }

        private static void ResetChipImmediate()
        {
            WriteCommand(0x81, 0x00, 0x00);
        }

        private static void WriteFmPcm(byte sample, byte wait)
        {
            WriteCommand(0x04, sample, wait);
        }

        private static void WriteCommand(byte command, byte arg1, byte arg2)
        {
            VgmState.Commands.Enqueue((uint)(
                ((command << 16) & 0xff0000) |
                ((arg1 << 8) & 0x00ff00) |
                (arg2 & 0x0000ff)
            ));

            _serial?.Write(stackalloc byte[3] { command, arg1, arg2 });
        }

        private static FileStream OpenSerial(string name)
        {
            return new FileStream(
                new SafeFileHandle(
                    CreateFile(
                        @"\\.\" + name,
                        0x40000000,
                        0,
                        IntPtr.Zero,
                        3,
                        0,
                        IntPtr.Zero
                    ),
                    true
                ),
                FileAccess.Write,
                3 * 16,
                false
            );
        }
    }
}
