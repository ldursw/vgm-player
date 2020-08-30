using System;

namespace VgmReader.Gui
{
    class VgmCommandParser
    {
        private static byte latchedPsgChannel;
        private static byte latchedPsgType;

        public static void Reset()
        {
            // reset FM
            VgmState.FmState.Reset();

            // silence PSG

            // channel 0 volume mute
            ParsePsg(0x9f);
            // channel 1 volume mute
            ParsePsg(0xbf);
            // channel 2 volume mute
            ParsePsg(0xdf);
            // channel 3 volume mute
            ParsePsg(0xff);
        }

        public static void ParsePsg(byte data)
        {
            if ((data & 0x80) > 0)
            {
                latchedPsgChannel = (byte)((data >> 5) & 0x03);
                latchedPsgType = (byte)((data >> 4) & 0x01);
                var psgData = (byte)(data & 0x0f);

                if (latchedPsgType == 1)
                {
                    var state = VgmState.PsgState[latchedPsgChannel];
                    var volume = (byte)(0x0f - psgData);

                    state.InternalVolume = volume;
                    state.Volume = Map(state.InternalVolume, 0, 0x0f);
                }
                else if (latchedPsgType == 0 && latchedPsgChannel == 3)
                {
                    var tone = (byte)(0x07 - (psgData & 0x07));

                    VgmState.PsgState[3].InternalTone = tone;
                    VgmState.PsgState[3].Tone = Map(tone, 0, 0x07);
                }
                else if (latchedPsgType == 0)
                {
                    var tone = (byte)((0x0f - psgData) & 0x0f);
                    var state = VgmState.PsgState[latchedPsgChannel];
                    state.InternalTone &= 0x3f0;
                    state.InternalTone |= tone;

                    state.Tone = Map(state.InternalTone, 0, 0x3ff);
                }
            }
            else
            {
                var psgData = (byte)(data & 0x3f);

                if (latchedPsgType == 1)
                {
                    var state = VgmState.PsgState[latchedPsgChannel];
                    var volume = (byte)(0x0f - psgData);

                    state.InternalVolume = volume;
                    state.Volume = Map(state.InternalVolume, 0, 0x0f);
                }
                else if (latchedPsgType == 0 && latchedPsgChannel == 3)
                {
                    var tone = (byte)(0x07 - (psgData & 0x07));

                    VgmState.PsgState[3].InternalTone = tone;
                    VgmState.PsgState[3].Tone = Map(tone, 0, 0x07);
                }
                else if (latchedPsgType == 0)
                {
                    var tone = (byte)((0x3f - psgData) & 0x3f);
                    var state = VgmState.PsgState[latchedPsgChannel];
                    state.InternalTone &= 0x0f;
                    state.InternalTone |= (ushort)(tone << 4);

                    state.Tone = Map(state.InternalTone, 0, 0x3ff);
                }
            }
        }

        public static void ParseFm(byte port, byte address, byte value)
        {
            VgmState.FmMap[(port * 0x100) + address] = value;

            if (port == 0 && address == 0x2a)
            {
                Array.Copy(VgmState.PcmSamples, 1, VgmState.PcmSamples, 0,
                    VgmState.PcmSamples.Length - 1);

                VgmState.PcmSamples[^1] = Map(value, 0, 255);
            }
            else if (port == 0 && address == 0x2b && value == 0)
            {
                Array.Fill<byte>(VgmState.PcmSamples, 50);
            }

            VgmState.FmState.Write(port, address, value);
        }

        private static byte Map(ushort x, ushort in_min, ushort in_max)
        {
            return (byte)((x - in_min) * 100 / (in_max - in_min));
        }
    }
}
