using System;
using VgmPlayer.Structs;

namespace VgmPlayer.Filters
{
    class PsgFilter
    {
        public static Span<VgmInstruction> FilterCommand(VgmInstruction inst,
            Span<VgmInstruction> buffer)
        {
            if (inst.Type == InstructionType.PsgWrite)
            {
                buffer[0] = inst;

                return buffer[0..1];
            }

            return Span<VgmInstruction>.Empty;
        }
    }
}
