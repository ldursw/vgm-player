using System;
using VgmReader.Gui;

namespace VgmPlayer.Gui.Elements
{
    class FmGraph : IGuiElement
    {
        public int X { get; set; }
        public int Y { get; set; }

        public FmGraph(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Draw(IntPtr renderer)
        {
            DrawFmMeter(renderer, X, Y, 0);
            DrawFmMeter(renderer, X + 60, Y, 1);
            DrawFmMeter(renderer, X + 120, Y, 2);
            DrawFmMeter(renderer, X + 180, Y, 3);
            DrawFmMeter(renderer, X + 240, Y, 4);
            DrawFmMeter(renderer, X + 300, Y, 5);

            VgmState.FmState.Update();
        }

        private static void DrawFmMeter(IntPtr renderer, int ax, int ay, int channel)
        {
            var ch = VgmState.FmState.Channels[channel];

            Font.Render(renderer, (channel + 1).ToString(), ax + 20, ay + 118, 0xffffff);

            if (((ch.pan >> 6) & 2) > 0)
            {
                Font.Render(renderer, "<", ax + 4, ay + 120, 0xff0000);
            }

            if (((ch.pan >> 6) & 1) > 0)
            {
                Font.Render(renderer, ">", ax + 36, ay + 120, 0xff0000);
            }

            for (var i = 0; i < ch.slots.Length; i++)
            {
                var slot = ch.slots[i];

                var x = ax + 5 + (12 * i);
                var y = ay;

                var (color1, color2) = slot.outSlot switch
                {
                    0 => (0x000055u, 0x0000ffu), // blue
                    1 => (0x003355u, 0x0077ffu), // blue/cyan
                    2 => (0x005555u, 0x00ddffu), // cyan
                    _ => (0x005500u, 0x00ff00u), // green
                };

                // bright = current env, dark = TL
                DrawFmMeter(
                    renderer,
                    x,
                    y,
                    slot.tl,
                    (ushort)slot.env,
                    color1,
                    color2
                );

                int freq;

                // special mode
                if (VgmState.FmState.Ch3Special && channel == 2)
                {
                    if (i == 3)
                    {
                        freq = VgmState.FmState.Channels[2].freq;
                    }
                    else
                    {
                        freq = VgmState.FmState.Channels[2 - i].ext_freq;
                    }
                }
                else
                {
                    freq = slot.mul > 0 ? ch.freq * slot.mul : ch.freq / 2;
                }

                // we assume "too high" frequency
                freq = Math.Min(freq, 0x3fff);

                var fy = Map((ushort)freq, 0x3fff, 0);
                Rect.DrawRectangle(renderer, x, y + fy, 10, 3, 0xff0000);
                if (slot.sl > slot.tl)
                {
                    var sy = Map(slot.sl, 0x3fff, 0);
                    Rect.DrawRectangle(renderer, x, y + sy, 10, 3, 0xbbbbbb);
                }
            }
        }

        private static void DrawFmMeter(IntPtr renderer, int x, int y, ushort backHeight,
                ushort valueHeight, uint backColor, uint valueColor)
        {
            const int width = 10;
            const int height = 100;

            var maxH = Math.Max(valueHeight, backHeight);

            var backH = Map(backHeight, 0, 0x1fff);
            var valH = maxH == 0 ? 0 : Map(maxH, 0, 0x1fff);

            Rect.DrawRectangle(renderer, x, y + backH, width, height - backH, backColor);
            Rect.DrawRectangle(renderer, x, y + valH, width, height - valH, valueColor);
        }

        private static byte Map(ushort x, ushort in_min, ushort in_max)
        {
            return (byte)((x - in_min) * 100 / (in_max - in_min));
        }
    }
}
