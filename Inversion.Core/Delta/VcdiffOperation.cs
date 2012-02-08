using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inversion.Delta
{
    public enum VcdiffOperation : byte
    {
        NoOp = 0,
        Add = 1,
        Run = 2,
        Copy = 3
    }
}
