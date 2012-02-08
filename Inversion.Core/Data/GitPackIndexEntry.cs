using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inversion.Data
{
    public class GitPackIndexEntry
    {
        public uint Offset { get; private set; }
        public byte[] Hash { get; private set; }

        public GitPackIndexEntry(uint offset, byte[] hash)
        {
            Offset = offset;
            Hash = hash;
        }
    }
}
