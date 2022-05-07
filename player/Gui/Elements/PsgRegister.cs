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
            Font.Render(renderer, "T1", X, Y, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[0].InternalTone, X + 48, Y, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[0].InternalVolume, X + 64, Y + 16, 0xffffff);

            Font.Render(renderer, "T2", X, Y + 48, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[1].InternalTone, X + 48, Y + 48, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[1].InternalVolume, X + 64, Y + 64, 0xffffff);

            Font.Render(renderer, "T3", X, Y + 96, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[2].InternalTone, X + 48, Y + 96, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[2].InternalVolume, X + 64, Y + 112, 0xffffff);

            Font.Render(renderer, "NS", X, Y + 144, 0xffffff);
            Font.Render(renderer, (byte)VgmState.PsgState[3].InternalTone, X + 48, Y + 144, 0xffffff);
            Font.Render(renderer, VgmState.PsgState[3].InternalVolume, X + 64, Y + 160, 0xffffff);
        }
    }
}
