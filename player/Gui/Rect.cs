// SPDX-License-Identifier: GPL-3.0
using System;
using static SDL2.SDL;

namespace VgmPlayer.Gui
{
    static class Rect
    {
        public static void DrawRectangle(IntPtr renderer, int x, int y, int w, int h,
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

        public static void DrawPoint(IntPtr renderer, int x, int y, uint color)
        {
            SDL_SetRenderDrawColor(
                renderer,
                (byte)((color >> 16) & 0xff),
                (byte)((color >> 8) & 0xff),
                (byte)((color >> 0) & 0xff),
                255
            );

            SDL_RenderDrawPoint(renderer, x, y);
        }

        public static void DrawPoints(IntPtr renderer, SDL_Point[] points, uint color)
        {
            SDL_SetRenderDrawColor(
                renderer,
                (byte)((color >> 16) & 0xff),
                (byte)((color >> 8) & 0xff),
                (byte)((color >> 0) & 0xff),
                255
            );

            SDL_RenderDrawPoints(renderer, points, points.Length);
        }
    }
}
