using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inversion.Data
{
    public class PackIndexEntry
    {
        public int Offset { get; private set; }
        public int Size { get; private set; }
        public byte[] Hash { get; private set; }

        public PackIndexEntry(int offset, int size, byte[] hash)
        {
            Offset = offset;
            Hash = hash;
        }
    }
}
