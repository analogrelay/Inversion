using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Inversion.Data
{
    internal class GitPackIndexV2 : GitPackIndex
    {
        private const uint FanoutStart = /* header */ (2 * 4);
        private const uint ShaTableStart = FanoutStart + /* fanout */ (256 * 4);
        private const uint OffsetTableMultiplier = ShaTableEntrySize + CrcTableEntrySize;
        private const uint ShaTableEntrySize = 20;
        private const uint CrcTableEntrySize = 4;

        private Func<FileAccess, Stream> _file;

        public override Version Version
        {
            get { return new Version(2, 0, 0, 0); }
        }

        public GitPackIndexV2(Func<FileAccess, Stream> file)
        {
            _file = file;
        }

        public override GitPackIndexEntry GetEntry(byte[] hash)
        {
            using (BinaryReader reader = new BinaryReader(_file(FileAccess.Read)))
            {
                // Get the value from the fanout table to figure out where to start
                RangeInfo range = GetFanout(reader, FanoutStart, hash[0]);

                // Seek to it
                reader.BaseStream.Seek(ShaTableStart + (range.Start * ShaTableEntrySize), SeekOrigin.Begin);

                // Start searching
                Tuple<uint, byte[]> shaEntry = FindShaEntry(reader, hash, range);
                if (shaEntry != null)
                {
                    return GetEntry(reader, shaEntry.Item1, range.TableLength, shaEntry.Item2);
                }
            }
            return null;
        }

        public override IEnumerable<GitPackIndexEntry> GetEntries()
        {
            using (BinaryReader reader = new BinaryReader(_file(FileAccess.Read)))
            {
                RangeInfo range = GetFanout(reader, FanoutStart, 0);

                // Read Shas
                reader.BaseStream.Seek(ShaTableStart, SeekOrigin.Begin);
                byte[][] shas = IterateShas(reader, 0, range.TableLength).Select(t => t.Item2).ToArray();

                // Read Offsets
                reader.BaseStream.Seek(ShaTableStart + (OffsetTableMultiplier * range.TableLength), SeekOrigin.Begin);
                for (int i = 0; i < range.TableLength; i++)
                {
                    uint start = reader.ReadNetworkUInt32();
                    yield return new GitPackIndexEntry(start, shas[i]);
                }
            }
        }

        private static Tuple<uint, byte[]> FindShaEntry(BinaryReader reader, byte[] hash, RangeInfo range)
        {
            foreach (Tuple<uint, byte[]> entry in IterateShas(reader, range.Start, range.End - range.Start))
            {
                if (entry.Item2.Take(hash.Length).SequenceEqual(hash))
                {
                    return entry;
                }
            }
            return null;
        }

        private static IEnumerable<Tuple<uint, byte[]>> IterateShas(BinaryReader reader, uint start, uint count)
        {
            for (uint i = 0; i < count; i++)
            {
                byte[] hash = reader.ReadBytes(Sha1HashSize);
                yield return Tuple.Create(start + i, hash);
            }
        }

        private static GitPackIndexEntry GetEntry(BinaryReader reader, uint index, uint tableLength, byte[] hash)
        {
            uint offsetStart = ShaTableStart + (OffsetTableMultiplier * tableLength);
            reader.BaseStream.Seek(offsetStart + (index * 4), SeekOrigin.Begin);
            uint start = reader.ReadNetworkUInt32();
            return new GitPackIndexEntry(start, hash);
        }
    }
}
