using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inversion.Delta
{
    public struct VcdiffCopyMode
    {
        public static readonly VcdiffCopyMode Self = new VcdiffCopyMode(VcdiffCopyModeType.Self);
        public static readonly VcdiffCopyMode Here = new VcdiffCopyMode(VcdiffCopyModeType.Here);
        public static readonly VcdiffCopyMode None = Self;
        
        public VcdiffCopyModeType Type { get; private set; }
        public byte Index { get; private set; }

        public VcdiffCopyMode(VcdiffCopyModeType type) : this(type, 0) { }
        public VcdiffCopyMode(VcdiffCopyModeType type, byte index) : this()
        {
            Type = type;
            Index = index;
        }

        public static VcdiffCopyMode Near(byte index)
        {
            return new VcdiffCopyMode(VcdiffCopyModeType.Near, index);
        }

        public static VcdiffCopyMode Same(byte index)
        {
            return new VcdiffCopyMode(VcdiffCopyModeType.Same, index);
        }

        public static VcdiffCopyMode Decode(byte value, byte nearTableSize)
        {
            switch (value)
            {
                case VcdiffConstants.VCD_SELF: return VcdiffCopyMode.Self;
                case VcdiffConstants.VCD_HERE: return VcdiffCopyMode.Here;
            }

            if (value > 1 && value <= nearTableSize + 1)
            {
                return VcdiffCopyMode.Near((byte)(value - 2));
            }
            else
            {
                return VcdiffCopyMode.Same((byte)(value - (nearTableSize + 2)));
            }
        }
    }
}
