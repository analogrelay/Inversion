using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inversion.Delta
{
    internal static class VcdiffConstants
    {
        public const byte VCD_DECOMPRESS = 0x02;
        public const byte VCD_CODETABLE = 0x01;
        public const byte VCD_TARGET = 0x02;
        public const byte VCD_SOURCE = 0x01;
        public const byte VCD_SELF = 0x00;
        public const byte VCD_HERE = 0x01;
        public const byte S_NEAR = 0x04;
        public const byte S_SAME = 0x03;

        public static readonly byte[] V0Header = new byte[] { 0xD6, 0xC3, 0xC4, 0x00 };
    }
}
