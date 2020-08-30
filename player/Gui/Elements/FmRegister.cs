// SPDX-License-Identifier: GPL-3.0
using System;
using VgmReader.Gui;

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
                var y = Y + (16 * (i / 16));
                var x = X + (i % 16 * 32);

                Font.Render(renderer, VgmState.FmMap[i], x, y,
                    (uint)(i % 2 == 0 ? 0xffffff : 0xbbbbbb));
            }

            for (var i = 0x31; i <= 0xb6; i++)
            {
                var y = Y + (16 * (i / 16));
                var x = X + (i % 16 * 32);

                Font.Render(renderer, VgmState.FmMap[i], x, y,
                    (uint)(i % 2 == 0 ? 0xffffff : 0xbbbbbb));
            }

            for (var i = 0x131; i <= 0x1b6; i++)
            {
                var y = Y + 208 + (16 * ((i - 0x131) / 16));
                var x = X + (i % 16 * 32);

                Font.Render(renderer, VgmState.FmMap[i], x, y,
                    (uint)(i % 2 == 0 ? 0xffffff : 0xbbbbbb));
            }
        }
    }
}
