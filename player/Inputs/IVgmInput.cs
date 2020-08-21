using System;
using System.Collections.Generic;

namespace VgmReader.Inputs
{
    interface IVgmInput : IDisposable
    {
        IEnumerable<uint> Instructions { get; }
    }
}
