using VgmReader.Gui;
using VgmReader.Inputs;
using VgmReader.Outputs;

namespace VgmReader
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
