// SPDX-License-Identifier: GPL-3.0
using System;
using System.Runtime.InteropServices;
using VgmPlayer.Structs;
using static SDL2.SDL;

namespace VgmPlayer.Gui
{
    class AudioQueue
    {
        private static uint _audioDev;

        public static void Initialize()
        {
            var audioSpec = new SDL_AudioSpec()
            {
                freq = 44100,
                format = AUDIO_S16,
                channels = 1,
                samples = 1,
                callback = OnAudioTick,
            };

            _audioDev = SDL_OpenAudioDevice(null, 0, ref audioSpec, out var _, 0);
            SDL_PauseAudioDevice(_audioDev, 0);
        }

        private static void OnAudioTick(IntPtr userdata, IntPtr stream, int len)
        {
            var stop = false;
            var gotPcm = false;

            while (stop || !VgmState.Commands.IsEmpty)
            {
                if (VgmState.WaitSamples > 0)
                {
                    VgmState.WaitSamples--;

                    break;
                }

                if (!VgmState.Commands.TryDequeue(out var item))
                {
                    break;
                }

                switch (item.Type)
                {
                    case InstructionType.PsgWrite:
                        VgmCommandParser.ParsePsg(item.Data1);
                        stop = true;
                        break;
                    case InstructionType.FmWrite0:
                        VgmCommandParser.ParseFm(0, item.Data1, item.Data2);
                        stop = true;
                        break;
                    case InstructionType.FmWrite1:
                        VgmCommandParser.ParseFm(1, item.Data1, item.Data2);
                        stop = true;
                        break;
                    case InstructionType.WaitSample:
                        VgmState.WaitSamples = (item.Data2 << 8) | item.Data1;

                        if (VgmState.WaitSamples <= 0)
                        {
                            stop = true;
                        }

                        break;
                    case InstructionType.End:
                    case InstructionType.ResetImmediate:
                        VgmCommandParser.Reset();
                        stop = true;
                        break;
                    case InstructionType.FmSample: // fm write pcm
                        VgmCommandParser.ParseFm(0, 0x2a, item.Data1);
                        VgmState.WaitSamples = item.Data2;
                        gotPcm = true;

                        if (VgmState.WaitSamples <= 0)
                        {
                            stop = true;
                        }

                        break;
                }
            }

            if (!gotPcm)
            {
                // Move values 1 sample to the left
                Array.Copy(VgmState.PcmSamples, 1, VgmState.PcmSamples, 0,
                    VgmState.PcmSamples.Length - 1);

                // Copy last sample
                VgmState.PcmSamples[^1] = VgmState.PcmSamples[^2];
            }

            // Write silence
            Marshal.WriteInt16(stream, 0, 0);
        }
    }
}
