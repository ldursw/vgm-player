// SPDX-License-Identifier: GPL-3.0
using System;

namespace VgmPlayer.Gui.Elements
{
    class PcmGraph : IGuiElement
    {
        private const int GraphSizeX = 100;
        private const int GraphSizeY = 128;
        private const int MinValue = 0;
        private const int MaxValue = 100;

        public int X { get; set; }
        public int Y { get; set; }

        public PcmGraph(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Draw(IntPtr renderer)
        {
            var samplesPerPixel = VgmState.PcmSamples.Length / GraphSizeY;
            var middleY = GetMiddleY();

            for (var i = 0; i < GraphSizeY; i++)
            {
                // https://stackoverflow.com/a/20190230
                var low = middleY;
                var high = middleY;
                for (var si = 0; si < samplesPerPixel; si++)
                {
                    var s = VgmState.PcmSamples[(i * samplesPerPixel) + si];
                    low = Math.Min(low, s);
                    high = Math.Max(high, s);
                }

                var x = X + i;
                var y1 = Y + (GraphSizeX * (low - MinValue) / MaxValue);
                var y2 = Y + (GraphSizeX * (high - MinValue) / MaxValue);

                Rect.DrawLine(renderer, x, y1, x, y2, 0xdddddd);
            }
        }

        private int GetMiddleY()
        {
            long middleY = VgmState.PcmSamples[0];
            for (int i = 1; i < VgmState.PcmSamples.Length; i++)
            {
                middleY += VgmState.PcmSamples[i];
            }

            middleY /= VgmState.PcmSamples.Length;

            return (int)middleY;
        }
    }
}
