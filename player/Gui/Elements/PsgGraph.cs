using System;
using VgmReader.Gui;

namespace VgmPlayer.Gui.Elements
{
    class PsgGraph : IGuiElement
    {
        public int X { get; set; }
        public int Y { get; set; }

        private const int Width = 20;
        private const int Height = 100;

        public PsgGraph(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Draw(IntPtr renderer)
        {
            DrawPsgMeter(renderer, X, Y, VgmState.PsgState[0]);
            DrawPsgMeter(renderer, X + Width + 5, Y, VgmState.PsgState[1]);
            DrawPsgMeter(renderer, X + Width * 2 + 10, Y, VgmState.PsgState[2]);
            DrawPsgMeter(renderer, X + Width * 3 + 15, Y, VgmState.PsgState[3]);
        }

        private static void DrawPsgMeter(IntPtr renderer, int x, int y, PsgState state)
        {
            DrawPsgMeter(renderer, x, y, state.Volume, state.Tone);
        }

        private static void DrawPsgMeter(IntPtr renderer, int x, int y, int volume, int tone)
        {
            Rect.DrawRectangle(renderer, x, y, Width, Height, 0x005500);
            Rect.DrawRectangle(renderer, x, y + (Height - volume), Width, volume, 0x00ff00);
            Rect.DrawRectangle(renderer, x, y + (Height - tone) - 2, Width, 4, 0xff0000);
        }
    }
}
