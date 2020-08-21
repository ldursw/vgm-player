using System;
using System.Runtime.InteropServices;
using VgmReader.Devices;
using static SDL2.SDL;

namespace VgmReader
{
    class Renderer
    {
        private const double FrameTime = 1000 / 60;

        private static uint lastRenderTime = 0;
        private static IntPtr window;
        private static IntPtr renderer;
        private static SDL_AudioSpec audioSpec;
        private static uint audioDev;

        // private static Sn76489 _sn76489;
        // private static byte _lastSample = 127;

        public static void Setup()
        {
            SDL_Init(SDL_INIT_VIDEO | SDL_INIT_AUDIO);

            window = SDL_CreateWindow(
                "VGM Player",
                SDL_WINDOWPOS_CENTERED,
                SDL_WINDOWPOS_CENTERED,
                800,
                600,
                SDL_WindowFlags.SDL_WINDOW_VULKAN
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

                var command = (byte)((item >> 16) & 0xff);
                var data1 = (byte)((item >> 8) & 0xff);
                var data2 = (byte)(item & 0xff);

                switch (command)
                {
                    case 0:
                    case 1: // fm write
                        VgmCommandParser.ParseFm(command, data1, data2);
                        stop = true;
                        break;
                    case 2: // psg write
                        VgmCommandParser.ParsePsg(data1);
                        // _sn76489.Write(data1);
                        stop = true;
                        break;
                    case 3: // wait
                        VgmState.WaitSamples = (data1 << 8) | data2;

                        if (VgmState.WaitSamples <= 0)
                        {
                            stop = true;
                        }

                        break;
                    case 4: // fm write pcm
                        VgmCommandParser.ParseFm(0, 0x2a, data1);
                        VgmState.WaitSamples = data2;
                        // _lastSample = data1;
                        gotPcm = true;

                        if (VgmState.WaitSamples <= 0)
                        {
                            stop = true;
                        }

                        break;
                    case 5:
                    case 0x81: // reset chip
                        VgmCommandParser.Reset();
                        stop = true;
                        break;
                }
            }

            if (!gotPcm)
            {
                Array.Copy(VgmState.PcmSamples, 1, VgmState.PcmSamples, 0,
                    VgmState.PcmSamples.Length - 1);

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
            DrawPsgMeter(renderer, 10, 10, VgmState.PsgState[0]);
            DrawPsgMeter(renderer, 35, 10, VgmState.PsgState[1]);
            DrawPsgMeter(renderer, 60, 10, VgmState.PsgState[2]);
            DrawPsgMeter(renderer, 85, 10, VgmState.PsgState[3]);

            DrawPcm(renderer, 10, 130);

            Font.Render(renderer, "PSG   T1   T2   T3   NS", 200, 10, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[0].InternalTone, 280, 30, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[0].InternalVolume, 296, 46, 0xffffff);

            Font.Render(renderer, VgmState.PsgState[1].InternalTone, 360, 30, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[1].InternalVolume, 376, 46, 0xffffff);

            Font.Render(renderer, VgmState.PsgState[2].InternalTone, 440, 30, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[2].InternalVolume, 456, 46, 0xffffff);

            Font.Render(renderer, (byte)VgmState.PsgState[3].InternalTone, 536, 30, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[3].InternalVolume, 536, 46, 0xffffff);

            for (var i = 0x22; i <= 0x2b; i++)
            {
                var y = 70 + (16 * (i / 16));
                var x = 200 + (i % 16 * 32);

                Font.Render(renderer, VgmState.FmMap[i], x, y,
                    (uint)(i % 2 == 0 ? 0xffffff : 0xbbbbbb));
            }

            for (var i = 0x31; i <= 0xb6; i++)
            {
                var y = 70 + (16 * (i / 16));
                var x = 200 + (i % 16 * 32);

                Font.Render(renderer, VgmState.FmMap[i], x, y,
                    (uint)(i % 2 == 0 ? 0xffffff : 0xbbbbbb));
            }

            for (var i = 0x131; i <= 0x1b6; i++)
            {
                var y = 70 + (16 * (i / 16));
                var x = 200 + (i % 16 * 32);

                Font.Render(renderer, VgmState.FmMap[i], x, y,
                    (uint)(i % 2 == 0 ? 0xffffff : 0xbbbbbb));
            }
        }

        private static void DrawPsgMeter(IntPtr renderer, int x, int y, PsgState state)
        {
            DrawPsgMeter(renderer, x, y, state.Volume, state.Tone);
        }

        private static void DrawPsgMeter(IntPtr renderer, int x, int y, int volume, int tone)
        {
            const int w = 20;
            const int h = 100;

            DrawRectangle(renderer, x, y, w, h, 0x005500);
            DrawRectangle(renderer, x, y + (h - volume), w, volume, 0x00ff00);
            DrawRectangle(renderer, x, y + (h - tone) - 2, w, 4, 0xff0000);
        }

        private static void DrawPcm(IntPtr renderer, int x, int y)
        {
            SDL_SetRenderDrawColor(
                renderer,
                0xdd,
                0xdd,
                0xdd,
                255
            );

            for (var i = 0; i < VgmState.PcmSamples.Length; i++)
            {
                var sample = VgmState.PcmSamples[i];

                SDL_RenderDrawPoint(renderer, x + i, y + sample);
                SDL_RenderDrawPoint(renderer, x + i, y + sample + 1);
            }
        }

        private static void DrawRectangle(IntPtr renderer, int x, int y, int w, int h,
            uint color)
        {
            SDL_SetRenderDrawColor(
                renderer,
                (byte)((color >> 16) & 0xff),
                (byte)((color >> 8) & 0xff),
                (byte)((color >> 0) & 0xff),
                255
            );

            var rect = new SDL_Rect() { x = x, y = y, w = w, h = h };
            SDL_RenderFillRect(renderer, ref rect);
        }
    }
}
