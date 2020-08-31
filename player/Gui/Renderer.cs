// SPDX-License-Identifier: GPL-3.0
using System;
using VgmPlayer.Gui.Elements;
using VgmPlayer.Outputs;
using static SDL2.SDL;

namespace VgmPlayer.Gui
{
    class Renderer
    {
        private const uint FrameTime = 1000 / 60;

        private static uint _lastRenderTime;
        private static IntPtr _window;
        private static IntPtr _renderer;
        private static readonly IGuiElement[] _elements;

        static Renderer()
        {
            _elements = new IGuiElement[]
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

            _window = SDL_CreateWindow(
                "VGM Player",
                SDL_WINDOWPOS_CENTERED,
                SDL_WINDOWPOS_CENTERED,
                800,
                600,
                SDL_WindowFlags.SDL_WINDOW_VULKAN |
                SDL_WindowFlags.SDL_WINDOW_RESIZABLE
            );

            _renderer = SDL_CreateRenderer(
                _window,
                -1,
                SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC |
                SDL_RendererFlags.SDL_RENDERER_ACCELERATED
            );

            SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "nearest");
            SDL_RenderSetLogicalSize(_renderer, 800, 600);

            Font.Initialize(_renderer);
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

            SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 255);
            SDL_RenderClear(_renderer);

            RenderWindow();

            SDL_RenderPresent(_renderer);

            SleepRenderer();

            return true;
        }

        private static void SleepRenderer()
        {
            // First run, set the current time.
            if (_lastRenderTime == 0)
            {
                _lastRenderTime = SDL_GetTicks();
            }

            // If the frame renders too fast, add a delay to
            // cap the framerate at SCREEN_FRAMERATE.
            var elapsedTime = SDL_GetTicks() - _lastRenderTime;
            if (elapsedTime < FrameTime)
            {
                SDL_Delay(FrameTime - elapsedTime);
            }

            _lastRenderTime = SDL_GetTicks();
        }

        private static void RenderWindow()
        {
            foreach (var element in _elements)
            {
                element.Draw(_renderer);
            }
        }
    }
}
