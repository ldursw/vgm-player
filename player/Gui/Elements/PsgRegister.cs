// SPDX-License-Identifier: GPL-3.0
using System;

namespace VgmPlayer.Gui.Elements
{
    class PsgRegister : IGuiElement
    {
        public int X { get; set; }
        public int Y { get; set; }

        public PsgRegister(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Draw(IntPtr renderer)
        {
            Font.Render(renderer, "PSG   T1   T2   T3   NS", X, Y, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[0].InternalTone, X + 80, Y + 20, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[0].InternalVolume, X + 96, Y + 36, 0xffffff);

            Font.Render(renderer, VgmState.PsgState[1].InternalTone, X + 160, Y + 20, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[1].InternalVolume, X + 176, Y + 36, 0xffffff);

            Font.Render(renderer, VgmState.PsgState[2].InternalTone, X + 240, Y + 20, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[2].InternalVolume, X + 256, Y + 36, 0xffffff);

            Font.Render(renderer, (byte)VgmState.PsgState[3].InternalTone, X + 336, Y + 20, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[3].InternalVolume, X + 336, Y + 36, 0xffffff);
        }
    }
}
