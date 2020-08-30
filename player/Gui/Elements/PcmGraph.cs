using System;
using VgmReader.Gui;

namespace VgmPlayer.Gui.Elements
{
    class PcmGraph : IGuiElement
    {
        public int X { get; set; }
        public int Y { get; set; }

        public PcmGraph(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Draw(IntPtr renderer)
        {
            for (var i = 0; i < VgmState.PcmSamples.Length; i++)
            {
                var sample = VgmState.PcmSamples[i];

                Rect.DrawPoint(renderer, X + i, Y + sample, 0xdddddd);
                Rect.DrawPoint(renderer, X + i, Y + sample + 1, 0xdddddd);
            }
        }
    }
}
