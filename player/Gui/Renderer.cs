using System;
using System.Runtime.InteropServices;
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
            DrawPsgMeter(renderer, 10, 10);
            DrawPcm(renderer, 10, 130);
            DrawFmMeter(renderer, 40, 10);

            // DrawPsgRegisters(renderer);
            // DrawFmRegisters(renderer);

            VgmState.FmState.Update();
        }

        private static void DrawPsgMeter(IntPtr renderer, int x, int y)
        {
            DrawPsgMeter(renderer, x, y, VgmState.PsgState[0]);
            DrawPsgMeter(renderer, x + 25, y, VgmState.PsgState[1]);
            DrawPsgMeter(renderer, x + 50, y, VgmState.PsgState[2]);
            DrawPsgMeter(renderer, x + 75, y, VgmState.PsgState[3]);
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

        private static void DrawPsgRegisters(IntPtr renderer)
        {
            Font.Render(renderer, "PSG   T1   T2   T3   NS", 200, 10, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[0].InternalTone, 280, 30, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[0].InternalVolume, 296, 46, 0xffffff);

            Font.Render(renderer, VgmState.PsgState[1].InternalTone, 360, 30, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[1].InternalVolume, 376, 46, 0xffffff);

            Font.Render(renderer, VgmState.PsgState[2].InternalTone, 440, 30, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[2].InternalVolume, 456, 46, 0xffffff);

            Font.Render(renderer, (byte)VgmState.PsgState[3].InternalTone, 536, 30, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[3].InternalVolume, 536, 46, 0xffffff);
        }

        private static void DrawFmRegisters(IntPtr renderer)
        {
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

        private static void DrawFmMeter(IntPtr renderer, int x, int y)
        {
            DrawFmMeter(renderer, x, y, 0);
            DrawFmMeter(renderer, x + 60, y, 1);
            DrawFmMeter(renderer, x + 120, y, 2);
            DrawFmMeter(renderer, x + 180, y, 3);
            DrawFmMeter(renderer, x + 240, y, 4);
            DrawFmMeter(renderer, x + 300, y, 5);
        }

        private static void DrawFmMeter(IntPtr renderer, int ax, int ay, int channel)
        {
            var ch = VgmState.FmState.Channels[channel];

            Font.Render(renderer, (channel + 1).ToString(), ax + 20, ay + 118, 0xffffff);

            if (((ch.pan >> 6) & 2) > 0)
            {
                Font.Render(renderer, "<", ax + 4, ay + 120, 0xff0000);
            }

            if (((ch.pan >> 6) & 1) > 0)
            {
                Font.Render(renderer, ">", ax + 36, ay + 120, 0xff0000);
            }

            for (var i = 0; i < ch.slots.Length; i++)
            {
                var slot = ch.slots[i];

                var x = ax + 5 + (12 * i);
                var y = ay;

                var (color1, color2) = slot.outSlot switch
                {
                    0 => (0x000055u, 0x0000ffu), // blue
                    1 => (0x003355u, 0x0077ffu), // blue/cyan
                    2 => (0x005555u, 0x00ddffu), // cyan
                    _ => (0x005500u, 0x00ff00u), // green
                };

                // bright = current env, dark = TL
                DrawFmMeter(
                    renderer,
                    x,
                    y,
                    slot.tl,
                    (ushort)slot.env,
                    color1,
                    color2
                );

                int freq;

                // special mode
                if (VgmState.FmState.Ch3Special && channel == 2)
                {
                    if (i == 3)
                    {
                        freq = VgmState.FmState.Channels[2].freq;
                    }
                    else
                    {
                        freq = VgmState.FmState.Channels[2 - i].ext_freq;
                    }
                }
                else
                {
                    freq = slot.mul > 0 ? ch.freq * slot.mul : ch.freq / 2;
                }

                // we assume "too high" frequency
                freq = Math.Min(freq, 0x3fff);

                var fy = Map((ushort)freq, 0x3fff, 0);
                DrawRectangle(renderer, x, y + fy, 10, 3, 0xff0000);
                if (slot.sl > slot.tl)
                {
                    var sy = Map(slot.sl, 0x3fff, 0);
                    DrawRectangle(renderer, x, y + sy, 10, 3, 0xbbbbbb);
                }
            }
        }

        private static void DrawFmMeter(IntPtr renderer, int x, int y, ushort backHeight,
                ushort valueHeight, uint backColor, uint valueColor)
        {
            const int width = 10;
            const int height = 100;

            var maxH = Math.Max(valueHeight, backHeight);

            var backH = Map(backHeight, 0, 0x1fff);
            var valH = maxH == 0 ? 0 : Map(maxH, 0, 0x1fff);

            DrawRectangle(renderer, x, y + backH, width, height - backH, backColor);
            DrawRectangle(renderer, x, y + valH, width, height - valH, valueColor);
        }

        private static byte Map(ushort x, ushort in_min, ushort in_max)
        {
            return (byte)((x - in_min) * 100 / (in_max - in_min));
        }
    }
}
