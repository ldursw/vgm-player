using System;

namespace VgmPlayer.Gui
{
    interface IGuiElement
    {
        int X { get; set; }
        int Y { get; set; }

        void Draw(IntPtr renderer);
    }
}
