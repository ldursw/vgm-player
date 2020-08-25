using System;
using System.Runtime.InteropServices;

namespace VgmPlayer.Uitls
{
    static class NativeMethods
    {
        [DllImport("kernel32", EntryPoint = "CreateFileW", SetLastError = true,
            CharSet = CharSet.Unicode, BestFitMapping = false, ExactSpelling = true)]
        public static extern IntPtr CreateFile(string lpFileName, int dwDesiredAccess,
            int dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition,
            int dwFlagsAndAttributes, IntPtr hTemplateFile);
    }
}
