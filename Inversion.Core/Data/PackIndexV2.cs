using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Inversion.Data
{
    internal class PackIndexV2 : PackIndex
    {
        private const uint FanoutStart = 8;
        private const uint TablesStart = /* header */ (2 * 4) + /* fanout */ (256 * 4);
        private const uint OffsetTableMultiplier = /* sha1 */ 20 + /* crc */ 4;

        private Func<FileAccess, Stream> _file;

        public override Version Version
        {
            get { return new Version(2, 0, 0, 0); }
        }

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
                Tuple<uint, byte[]> shaEntry = FindShaEntry(reader, hash, range);
                if (shaEntry != null)
                {
                    return GetEntry(reader, shaEntry.Item1, range.TableLength, shaEntry.Item2);
                }
            }
            return null;
        }

        public override IEnumerable<PackIndexEntry> GetEntries()
        {
            using (BinaryReader reader = new BinaryReader(_file(FileAccess.Read)))
            {
                RangeInfo range = GetFanout(reader, FanoutStart, 0);

                // Read Shas
                reader.BaseStream.Seek(TablesStart, SeekOrigin.Begin);
                byte[][] shas = IterateShas(reader, 0, range.TableLength).Select(t => t.Item2).ToArray();

                // Read Offsets
                reader.BaseStream.Seek(TablesStart + (OffsetTableMultiplier * range.TableLength), SeekOrigin.Begin);
                for (int i = 0; i < range.TableLength; i++)
                {
                    uint start = reader.ReadNetworkUInt32();
                    yield return new PackIndexEntry(start, shas[i]);
                }
            }
        }

        private static Tuple<uint, byte[]> FindShaEntry(BinaryReader reader, byte[] hash, RangeInfo range)
        {
            foreach (Tuple<uint, byte[]> entry in IterateShas(reader, range.Start, range.End))
            {
                if (entry.Item2.SequenceEqual(hash))
                {
                    return entry;
                }
            }
            return null;
        }

        private static IEnumerable<Tuple<uint, byte[]>> IterateShas(BinaryReader reader, uint start, uint count)
        {
            uint position = start;
            for (int i = 0; i < count; i++)
            {
                byte[] hash = reader.ReadBytes(Sha1HashSize);
                yield return Tuple.Create(position / 20, hash);
            }
        }

        private static PackIndexEntry GetEntry(BinaryReader reader, uint index, uint tableLength, byte[] hash)
        {
            uint offsetStart = TablesStart + (OffsetTableMultiplier * tableLength);
            reader.BaseStream.Seek(offsetStart + (index * 4), SeekOrigin.Begin);
            uint start = reader.ReadNetworkUInt32();
            return new PackIndexEntry(start, hash);
        }
    }
}
