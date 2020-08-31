// SPDX-License-Identifier: GPL-3.0
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using VgmPlayer.Structs;
using VgmPlayer.Uitls;
using VgmPlayer.Gui;
using VgmPlayer.Inputs;

namespace VgmPlayer.Outputs
{
    class VgmSerial
    {
        private static Stream? _serial;

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

            WriteCommand(VgmInstruction.ResetImmediate());

            foreach (var inst in vgm.Instructions)
            {
                WriteCommand(inst);
            }

            WriteCommand(VgmInstruction.End());

            vgm.Dispose();
            _serial?.Flush();
        }

        public static void Stop()
        {
            if (_serial != null)
            {
                var sp = _serial;
                _serial = null;

                SendData(sp, VgmInstruction.ResetImmediate());
                sp.Flush();
                sp.Dispose();
            }
        }

        private static void WriteCommand(VgmInstruction instruction)
        {
            VgmState.Commands.Enqueue(instruction);

            if (_serial != null)
            {
                SendData(_serial, instruction);
            }
        }

        private static void SendData(Stream stream, VgmInstruction instruction)
        {
            stream.Write(stackalloc byte[3]
            {
                (byte)instruction.Type,
                instruction.Data1,
                instruction.Data2
            });
        }

        private static Stream OpenSerial(string name)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                OpenSerialWindows(name) : OpenSerialLinux(name);
        }

        private static Stream OpenSerialWindows(string name)
        {
            return new FileStream(
                new SafeFileHandle(
                    NativeMethods.CreateFile(
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
                3 * 16, // buffer 16 3-byte commands
                false
            );
        }

        private static Stream OpenSerialLinux(string name)
        {
            // set Teensy mode as raw to prevent the OS
            // from sending control data
            Process.Start("stty", $"-F /dev/{name} raw").WaitForExit();

            return new FileStream(
                "/dev/" + name,
                FileMode.Open,
                FileAccess.Write,
                FileShare.ReadWrite,
                3 * 16, // buffer 16 3-byte commands
                false
            );
        }
    }
}
