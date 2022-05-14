using System;
using VgmPlayer.Structs;

namespace VgmPlayer.Filters
{
    struct WaitFilter
    {
        public static Span<VgmInstruction> FilterCommand(VgmInstruction inst,
            Span<VgmInstruction> buffer)
        {
            if (inst.Type == InstructionType.End ||
                inst.Type == InstructionType.ResetImmediate ||
                inst.Type == InstructionType.WaitSample)
            {
                buffer[0] = inst;

                return buffer[0..1];
            }

            return Span<VgmInstruction>.Empty;
        }
    }
}
