using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Inversion.Delta
{
    public class VcdiffCodeTable
    {
        public static readonly VcdiffCodeTable Default = CreateDefaultTable();

        private VcdiffCodeTableEntry[] _entries;

        public VcdiffCodeTableEntry this[int index]
        {
            get
            {
                return _entries[index];
            }
        }

        public VcdiffCodeTable(VcdiffCodeTableEntry[] entries)
        {
            Debug.Assert(entries.Length == 256);
            if (entries.Length != 256)
            {
                throw new InvalidOperationException("Cannot create a VCDIFF code table with more than 256 entries");
            }
            _entries = entries;
        }

        private static VcdiffCodeTable CreateDefaultTable()
        {
            VcdiffCodeTableEntry[] entries = new VcdiffCodeTableEntry[256];

            int ptr = 0;
            entries[ptr++] = new VcdiffCodeTableEntry(VcdiffOperation.Run, 0, VcdiffCopyMode.None);
            for (int i = 0; i < 18; i++)
            {
                entries[ptr++] = new VcdiffCodeTableEntry(VcdiffOperation.Add, (byte)(i - 1), VcdiffCopyMode.None);
            }
            Debug.Assert(ptr == 19);
            for (int mode = 0; mode <= 8; mode++)
            {
                for (int copySize = 0; copySize <= 15; copySize++)
                {
                    byte size = (byte)(copySize == 0 ? 0 : copySize + 2);
                    entries[ptr++] = new VcdiffCodeTableEntry(VcdiffOperation.Copy, size, VcdiffCopyMode.Decode((byte)mode, VcdiffConstants.S_NEAR));
                }
            }
            Debug.Assert(ptr == 163);
            for (int mode = 0; mode <= 5; mode++)
            {
                for (int addSize = 1; addSize <= 4; addSize++)
                {
                    for (int copySize = 4; copySize <= 6; copySize++)
                    {
                        entries[ptr++] = new VcdiffCodeTableEntry(VcdiffOperation.Add, (byte)addSize, VcdiffCopyMode.None, VcdiffOperation.Copy, (byte)copySize, VcdiffCopyMode.Decode((byte)mode, VcdiffConstants.S_NEAR));
                    }
                }
            }
            Debug.Assert(ptr == 235);
            for (int mode = 6; mode <= 8; mode++)
            {
                for (int addSize = 1; addSize <= 4; addSize++)
                {
                    entries[ptr++] = new VcdiffCodeTableEntry(VcdiffOperation.Add, (byte)addSize, VcdiffCopyMode.None, VcdiffOperation.Copy, 4, VcdiffCopyMode.Decode((byte)mode, VcdiffConstants.S_NEAR));
                }
            }
            Debug.Assert(ptr == 247);
            for (int mode = 0; mode <= 8; mode++)
            {
                entries[ptr++] = new VcdiffCodeTableEntry(VcdiffOperation.Copy, 4, VcdiffCopyMode.Decode((byte)mode, VcdiffConstants.S_NEAR), VcdiffOperation.Add, 1, VcdiffCopyMode.None);
            }
            Debug.Assert(ptr == 256);

            return new VcdiffCodeTable(entries);
        }
    }
}
