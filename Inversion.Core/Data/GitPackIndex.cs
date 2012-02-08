using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Inversion.Storage;
using Inversion.Utils;

namespace Inversion.Data
{
    public abstract class GitPackIndex
    {
        private static readonly byte[] V2PlusSignature = new byte[] { 255, 116, 79, 99 };
        internal const int Sha1HashSize = 20;

        public abstract Version Version { get; }

        public static GitPackIndex Open(IFileSystem fs, string path)
        {
            return Open(access => fs.Open(path, access, create: false));
        }

        public static GitPackIndex Open(Func<FileAccess, Stream> fileOpener)
        {
            using (BinaryReader rdr = new BinaryReader(fileOpener(FileAccess.Read)))
            {
                byte[] header = (byte[])rdr.ReadBytes(4);
                if (Enumerable.SequenceEqual(header, V2PlusSignature))
                {
                    uint ver = rdr.ReadNetworkUInt32();
                    switch (ver)
                    {
                        case 2:
                            return new GitPackIndexV2(fileOpener);
                        default:
                            throw new InvalidDataException(String.Format("Unknown Pack File version: '{0}'", ver));
                    }
                }
                return new GitPackIndexV1(fileOpener);
            }
        }

        public abstract GitPackIndexEntry GetEntry(byte[] hash);
        public abstract IEnumerable<GitPackIndexEntry> GetEntries();

        public virtual bool EntryExists(byte[] hash)
        {
            return GetEntry(hash) != null;
        }

        protected RangeInfo GetFanout(BinaryReader reader, uint fanoutStart, byte startIndex)
        {
            uint start;
            uint end;
            uint size;
            if (startIndex == 0)
            {
                reader.BaseStream.Seek(fanoutStart + (startIndex * 4), SeekOrigin.Begin);
                start = 0;
                end = reader.ReadNetworkUInt32();
            }
            else
            {
                reader.BaseStream.Seek(fanoutStart + ((startIndex - 1) * 4), SeekOrigin.Begin);
                start = reader.ReadNetworkUInt32();
                end = reader.ReadNetworkUInt32();
            }
            size = GetEntryCount(reader, fanoutStart);
            return new RangeInfo(start, end, size);
        }

        protected static uint GetEntryCount(BinaryReader reader, uint fanoutStart)
        {
            reader.BaseStream.Seek(fanoutStart + (255 * 4), SeekOrigin.Begin);
            return reader.ReadNetworkUInt32();
        }

        protected struct RangeInfo
        {
            public uint Start { get; private set; }
            public uint End { get; private set; }
            public uint TableLength { get; private set; }

            public RangeInfo(uint start, uint end, uint tableLength)
                : this()
            {
                Start = start;
                End = end;
                TableLength = tableLength;
            }
        }
    }
}
