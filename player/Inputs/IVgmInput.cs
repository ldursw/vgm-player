using System;
using System.Collections.Generic;
using VgmPlayer.Structs;

namespace VgmReader.Inputs
{
    interface IVgmInput : IDisposable
    {
        IEnumerable<VgmInstruction> Instructions { get; }
    }
}
