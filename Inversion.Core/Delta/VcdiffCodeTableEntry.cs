using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inversion.Delta
{
    public struct VcdiffCodeTableEntry
    {
        public VcdiffOperation Operation1 { get; private set; }
        public byte Size1 { get; private set; }
        public VcdiffCopyMode Mode1 { get; private set; }

        public VcdiffOperation Operation2 { get; private set; }
        public byte Size2 { get; private set; }
        public VcdiffCopyMode Mode2 { get; private set; }

        public VcdiffCodeTableEntry(VcdiffOperation operation, byte size, VcdiffCopyMode mode) : this(operation, size, mode, VcdiffOperation.NoOp, 0, VcdiffCopyMode.None) { }
        public VcdiffCodeTableEntry(VcdiffOperation operation1, byte size1, VcdiffCopyMode mode1, VcdiffOperation operation2, byte size2, VcdiffCopyMode mode2) : this()
        {
            Operation1 = operation1;
            Size1 = size1;
            Mode1 = mode1;
            Operation2 = operation2;
            Size2 = size2;
            Mode2 = mode2;
        }
    }
}
