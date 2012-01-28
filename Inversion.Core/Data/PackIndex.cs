using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Inversion.Data
{
    public abstract class PackIndex
    {
        internal const int Sha1HashSize = 20;
        public static PackIndex Open(Func<FileAccess, Stream> fileOpener)
        {
            using (BinaryReader rdr = new BinaryReader(fileOpener(FileAccess.Read)))
            {
                if (rdr.ReadChars(4) == "\xFFtOc".ToCharArray())
                {
                    int ver = rdr.ReadNetworkInt32();
                    switch (ver)
                    {
                        case 2:
                            return new PackIndexV2(fileOpener);
                        default:
                            throw new InvalidDataException(String.Format("Unknown Pack File version: '{0}'", ver));
                    }
                }
                return new PackIndexV1(fileOpener);
            }
        }

        public abstract PackIndexEntry GetEntry(byte[] hash);

        public virtual bool EntryExists(byte[] hash)
        {
            return GetEntry(hash) != null;
        }

        protected RangeInfo GetFanout(BinaryReader reader, int fanoutStart, byte startIndex)
        {
            int start;
            int end;
            int size;
            if (startIndex == 0)
            {
                reader.BaseStream.Seek(fanoutStart + (startIndex * 4), SeekOrigin.Begin);
                start = 0;
                end = reader.ReadNetworkInt32();
            }
            else
            {
                reader.BaseStream.Seek(fanoutStart + ((startIndex - 1) * 4), SeekOrigin.Begin);
                start = reader.ReadNetworkInt32();
                end = reader.ReadNetworkInt32();
            }
            reader.BaseStream.Seek(fanoutStart + (255 * 4), SeekOrigin.Begin);
            size = reader.ReadNetworkInt32();
            return new RangeInfo(start, end, size);
        }

        protected struct RangeInfo
        {
            public int Start { get; private set; }
            public int End { get; private set; }
            public int TableLength { get; private set; }

            public RangeInfo(int start, int end, int tableLength) : this()
            {
                Start = start;
                End = end;
                TableLength = tableLength;
            }
        }
    }
}
