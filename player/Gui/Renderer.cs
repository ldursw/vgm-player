// SPDX-License-Identifier: GPL-3.0
using System;
using VgmPlayer.Gui;
using VgmPlayer.Gui.Elements;
using VgmPlayer.Outputs;
using static SDL2.SDL;

namespace VgmPlayer.Gui
{
    class Renderer
    {
        private const double FrameTime = 1000 / 60;

        private static uint lastRenderTime = 0;
        private static IntPtr window;
        private static IntPtr renderer;
        private static readonly IGuiElement[] elements;

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
            AudioQueue.Initialize();
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
