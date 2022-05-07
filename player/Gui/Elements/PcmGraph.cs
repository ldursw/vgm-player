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

        private readonly int _samplesPerPixel;

        public PcmGraph(int x, int y)
        {
            X = x;
            Y = y;
            _samplesPerPixel = VgmState.PcmSamples.Length / GraphSizeY;
        }

        public void Draw(IntPtr renderer)
        {
            for (var i = 0; i < GraphSizeY; i++)
            {
                // https://stackoverflow.com/a/20190230
                int? low = null;
                int? high = null;
                for (var si = 0; si < _samplesPerPixel; si++)
                {
                    var s = VgmState.PcmSamples[(i * _samplesPerPixel) + si];
                    low = Math.Min(low ?? s, s);
                    high = Math.Max(high ?? s, s);
                }

                if (high == null || low == null || high == low)
                {
                    high = 50;
                    low = 50;
                }

                var x = X + i;
                int y1 = Y + (GraphSizeX * (low.Value - MinValue) / MaxValue);
                var y2 = Y + (GraphSizeX * (high.Value - MinValue) / MaxValue);

                Rect.DrawLine(renderer, x, y1, x, y2, 0xdddddd);
            }
        }
    }
}
