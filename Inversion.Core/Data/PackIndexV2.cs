using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Inversion.Data
{
    class PackIndexV2 : PackIndex
    {
        private const int FanoutStart = 8;
        
        private Func<FileAccess, Stream> _file;

        public PackIndexV2(Func<FileAccess, Stream> file)
        {
            _file = file;
        }

        public override PackIndexEntry GetEntry(byte[] hash)
        {
            Debug.Assert(hash.Length == 256);
            using (BinaryReader reader = new BinaryReader(_file(FileAccess.Read)))
            {
                // Get the value from the fanout table to figure out where to start
                RangeInfo range = GetFanout(reader, FanoutStart, hash[0]);

                // Seek to it
                reader.BaseStream.Seek(range.Start, SeekOrigin.Begin);

                // Start searching
                Tuple<int, byte[]> shaEntry = FindShaEntry(reader, hash, range);
                if (shaEntry != null)
                {
                    return GetEntry(reader, shaEntry.Item1, range.TableLength, shaEntry.Item2);
                }
            }
            return null;
        }

        private static Tuple<int, byte[]> FindShaEntry(BinaryReader reader, byte[] hash, RangeInfo range)
        {
            foreach (Tuple<int, byte[]> entry in IterateShas(reader, range.Start, range.End))
            {
                if (entry.Item2.SequenceEqual(hash))
                {
                    return entry;
                }
            }
            return null;
        }

        private static IEnumerable<Tuple<int, byte[]>> IterateShas(BinaryReader reader, int start, int count)
        {
            int position = start;
            for (int i = 0; i < count; i++)
            {
                byte[] hash = reader.ReadBytes(Sha1HashSize);
                yield return Tuple.Create(position / 20, hash);
            }
        }

        private static PackIndexEntry GetEntry(BinaryReader reader, int index, int tableLength, byte[] hash)
        {
            const int tablesStart = /* header */ (2 * 4) + /* fanout */ (256 * 4);
            const int tablesMult = /* sha1 */ 20 + /* crc */ 4;
            int offsetStart = tablesStart + (tablesMult * tableLength);
            reader.BaseStream.Seek(offsetStart + (index * 4), SeekOrigin.Begin);
            int start = reader.ReadNetworkInt32();
            int end = -1;
            if (index < tableLength - 1)
            {
                end = reader.ReadNetworkInt32();
            }
            return new PackIndexEntry(start, end < 0 ? end : (end - start), hash);
        }
    }
}
