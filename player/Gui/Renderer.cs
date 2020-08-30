using System;
using System.Runtime.InteropServices;
using VgmPlayer.Gui;
using VgmPlayer.Gui.Elements;
using VgmPlayer.Structs;
using VgmReader.Devices;
using VgmReader.Outputs;
using static SDL2.SDL;

namespace VgmReader.Gui
{
    class Renderer
    {
        private const double FrameTime = 1000 / 60;

        private static uint lastRenderTime = 0;
        private static IntPtr window;
        private static IntPtr renderer;
        private static SDL_AudioSpec audioSpec;
        private static uint audioDev;
        private static readonly IGuiElement[] elements;

        // private static Sn76489 _sn76489;
        // private static byte _lastSample = 127;

        static Renderer()
        {
            elements = new IGuiElement[]
            {
                new FmGraph(40, 10),
                new PcmGraph(450, 10),
                new PsgGraph(640, 10),
                // new PsgRegister(200, 160),
                // new FmRegister(140, 200),
            };
        }

        public static void Setup()
        {
            SDL_Init(SDL_INIT_VIDEO | SDL_INIT_AUDIO);

            window = SDL_CreateWindow(
                "VGM Player",
                SDL_WINDOWPOS_CENTERED,
                SDL_WINDOWPOS_CENTERED,
                800,
                600,
                SDL_WindowFlags.SDL_WINDOW_VULKAN |
                SDL_WindowFlags.SDL_WINDOW_RESIZABLE
            );

            renderer = SDL_CreateRenderer(
                window,
                -1,
                SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC |
                SDL_RendererFlags.SDL_RENDERER_ACCELERATED
            );

            SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "nearest");
            SDL_RenderSetLogicalSize(renderer, 800, 600);

            Font.Initialize(renderer);

            audioSpec = new SDL_AudioSpec()
            {
                freq = 44100,
                format = AUDIO_S16,
                channels = 1,
                samples = 1,
                callback = OnAudioTick,
            };
            audioDev = SDL_OpenAudioDevice(null, 0, ref audioSpec, out var _, 0);
            SDL_PauseAudioDevice(audioDev, 0);

            // _sn76489 = new Sn76489();
        }

        public static bool Loop()
        {
            while (SDL_PollEvent(out var e) != 0)
            {
                if (e.type == SDL_EventType.SDL_QUIT)
                {
                    VgmSerial.Stop();

                    return false;
                }
            }

            SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
            SDL_RenderClear(renderer);

            RenderWindow(renderer);

            SDL_RenderPresent(renderer);

            SleepRenderer();

            return true;
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
                        // _sn76489.Write(item.Data1);
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
                        // _lastSample = item.Data1;
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

            // var sample = (short)_sn76489.GetSample();
            // sample += (short)((_lastSample - 127) * 128);
            // Marshal.WriteInt16(stream, 0, sample);
            Marshal.WriteInt16(stream, 0, 0);
        }

        private static void SleepRenderer()
        {
            // First run, set the current time.
            if (lastRenderTime == 0)
            {
                lastRenderTime = SDL_GetTicks();
            }

            // If the frame renders too fast, add a delay to
            // cap the framerate at SCREEN_FRAMERATE.
            var elapsedTime = SDL_GetTicks() - lastRenderTime;
            if (elapsedTime < FrameTime)
            {
                SDL_Delay((uint)(FrameTime - elapsedTime));
            }

            lastRenderTime = SDL_GetTicks();
        }

        private static void RenderWindow(IntPtr renderer)
        {
            foreach (var element in elements)
            {
                element.Draw(renderer);
            }
        }
    }
}
