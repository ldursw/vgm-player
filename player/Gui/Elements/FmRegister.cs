// SPDX-License-Identifier: GPL-3.0
using System;
using VgmPlayer.Outputs;

namespace VgmPlayer.Gui.Elements
{
    class FmRegister : IGuiElement
    {
        public int X { get; set; }
        public int Y { get; set; }

        public FmRegister(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Draw(IntPtr renderer)
        {
            for (var i = 0x22; i <= 0x2b; i++)
            {
                if (!FmFilter.IsValidFmRegister(0, (byte)i))
                {
                    continue;
                }

                var y = Y + (16 * (i / 16));
                var x = X + (i % 16 * 32);
                var color = (uint)(i % 2 == 0 ? 0xffffff : 0xbbbbbb);

                Font.Render(renderer, VgmState.FmMap[i], x, y, color);
            }

            for (var i = 0x30; i <= 0xb6; i++)
            {
                if (!FmFilter.IsValidFmRegister(0, (byte)i))
                {
                    continue;
                }

                var y = Y + (16 * (i / 16));
                var x = X + (i % 16 * 32);
                var color = (uint)(i % 2 == 0 ? 0xffffff : 0xbbbbbb);

                Font.Render(renderer, VgmState.FmMap[i], x, y + 16, color);
                Font.Render(renderer, VgmState.FmMap[i | 0x100], x, y + 176, color);
            }
        }
    }
}
