// SPDX-License-Identifier: GPL-3.0
using System;
using static SDL2.SDL;

namespace VgmReader.Gui
{
    class Font
    {
        private const int CharWidth = 8;
        private const int CharHeight = 8;
        private const int CharsPerLine = 16;
        private static IntPtr _fontTexture;

        public static void Initialize(IntPtr renderer)
        {
            var surface = SDL_LoadBMP("font_default.bmp");
            _fontTexture = SDL_CreateTextureFromSurface(renderer, surface);
            SDL_FreeSurface(surface);
        }

        public static void Render(IntPtr renderer, ushort value, int x, int y, uint color)
        {
            Span<char> span = stackalloc char[4];
            value.TryFormat(span, out var length, "X3");
            Render(renderer, span, length, x, y, color);
        }

        public static void Render(IntPtr renderer, byte value, int x, int y, uint color)
        {
            Span<char> span = stackalloc char[2];
            value.TryFormat(span, out var length, "X2");
            Render(renderer, span, length, x, y, color);
        }

        public static void Render(IntPtr renderer, string text, int x, int y, uint color)
        {
            for (var i = 0; i < text.Length; i++)
            {
                Render(renderer, text[i], x + (16 * i), y, color);
            }
        }

        public static void Render(IntPtr renderer, Span<char> text, int length,
            int x, int y, uint color)
        {
            for (var i = 0; i < length; i++)
            {
                Render(renderer, text[i], x + (16 * i), y, color);
            }
        }

        public static void Render(IntPtr renderer, char letter, int x, int y, uint color)
        {
            var idx = letter - ' ';
            if (idx < 0 || idx > 95)
            {
                return;
            }

            var srcRect = new SDL_Rect()
            {
                x = idx % CharsPerLine * CharWidth,
                y = idx / CharsPerLine * CharHeight,
                w = CharWidth,
                h = CharHeight,
            };

            var destRect = new SDL_Rect()
            {
                x = x,
                y = y,
                w = 16,
                h = 16,
            };

            SDL_SetTextureColorMod(
                _fontTexture,
                (byte)((color >> 16) & 0xff),
                (byte)((color >> 8) & 0xff),
                (byte)((color >> 0) & 0xff)
            );
            SDL_RenderCopy(renderer, _fontTexture, ref srcRect, ref destRect);
        }
    }
}
