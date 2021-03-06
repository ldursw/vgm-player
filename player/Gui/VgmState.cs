// SPDX-License-Identifier: GPL-3.0
using System.Collections.Concurrent;
using VgmPlayer.Structs;

namespace VgmPlayer.Gui
{
    class VgmState
    {
        public static int WaitSamples { get; set; }
        public static ConcurrentQueue<VgmInstruction> Commands { get; }
        public static PsgState[] PsgState { get; }
        public static byte[] PcmSamples { get; }
        public static byte[] FmMap { get; }
        public static YmState FmState { get; }

        static VgmState()
        {
            Commands = new ConcurrentQueue<VgmInstruction>();

            PsgState = new PsgState[4];
            for (var i = 0; i < PsgState.Length; i++)
            {
                PsgState[i] = new PsgState();
            }

            PcmSamples = new byte[128 * 80];
            for (var i = 0; i < PcmSamples.Length; i++)
            {
                PcmSamples[i] = 50;
            }

            FmMap = new byte[0x200];
            FmState = new YmState();
        }
    }

    public class PsgState
    {
        public ushort InternalTone { get; set; }
        public byte InternalVolume { get; set; }

        public byte Tone { get; set; }
        public byte Volume { get; set; }
    }
}
