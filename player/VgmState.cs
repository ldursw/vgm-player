using System.Collections.Concurrent;

namespace VgmReader
{
    class VgmState
    {
        public static int WaitSamples { get; set; }
        public static ConcurrentQueue<uint> Commands { get; }
        public static PsgState[] PsgState { get; }
        public static byte[] PcmSamples { get; }
        public static byte[] FmMap { get; }

        static VgmState()
        {
            Commands = new ConcurrentQueue<uint>();

            PsgState = new PsgState[4];
            for (var i = 0; i < PsgState.Length; i++)
            {
                PsgState[i] = new PsgState();
            }

            PcmSamples = new byte[128];
            for (var i = 0; i < PcmSamples.Length; i++)
            {
                PcmSamples[i] = 50;
            }

            FmMap = new byte[0x200];
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
