using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace Inversion.Data
{
    internal class PackIndexV1 : PackIndex
    {
        private const int FanoutStart = 0;

        private Func<FileAccess, Stream> _file;

        public PackIndexV1(Func<FileAccess, Stream> file)
        {
            _file = file;
        }

        public override PackIndexEntry GetEntry(byte[] hash)
        {
            Debug.Assert(hash.Length == 256);
            using(BinaryReader reader = new BinaryReader(_file(FileAccess.Read))) {
                // Get the value from the fanout table to figure out where to start
                RangeInfo range = GetFanout(reader, FanoutStart, hash[0]);

                // Seek to it
                reader.BaseStream.Seek(range.Start, SeekOrigin.Begin);

                // Start searching
                foreach (PackIndexEntry entry in IterateEntries(reader, range.End))
                {
                    if (entry.Hash.SequenceEqual(hash))
                    {
                        return entry;
                    }
                }
                return null;
            }
        }

        private IEnumerable<PackIndexEntry> IterateEntries(BinaryReader reader, int count)
        {
            int offset = reader.ReadNetworkInt32();
            for (int i = 0; i < count; i++)
            {
                byte[] hash = reader.ReadBytes(Sha1HashSize);
                int nextOffset = reader.ReadNetworkInt32();
                yield return new PackIndexEntry(offset, nextOffset - offset, hash);
                offset = nextOffset;
            }
        }
    }
}
