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

        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

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

        public override IEnumerable<PackIndexEntry> GetEntries()
        {
            using (BinaryReader reader = new BinaryReader(_file(FileAccess.Read)))
            {
                return IterateEntries(reader, GetEntryCount(reader, FanoutStart)).ToArray();
            }
        }

        private IEnumerable<PackIndexEntry> IterateEntries(BinaryReader reader, uint count)
        {
            for (int i = 0; i < count; i++)
            {
                uint offset = reader.ReadNetworkUInt32();
                byte[] hash = reader.ReadBytes(Sha1HashSize);
                yield return new PackIndexEntry(offset, hash);
            }
        }
    }
}
