using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inversion.Data
{
    public class GitPackIndexEntry
    {
        public long Offset { get; private set; }
        public byte[] Hash { get; private set; }

        public GitPackIndexEntry(long offset, byte[] hash)
        {
            Offset = offset;
            Hash = hash;
        }
    }
}
