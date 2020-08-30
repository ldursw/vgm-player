// SPDX-License-Identifier: GPL-3.0
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
