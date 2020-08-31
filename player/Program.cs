// SPDX-License-Identifier: GPL-3.0
using VgmPlayer.Gui;
using VgmPlayer.Inputs;
using VgmPlayer.Outputs;

namespace VgmPlayer
{
    class Program
    {
        static void Main()
        {
            Renderer.Setup();

            VgmSerial.Play(
                "COM10",
                new VgmFile("music.vgm")
                // new VgmPipe()
                // new VgmPCM("music.pcm")
            );

            while (Renderer.Loop());
        }
    }
}
