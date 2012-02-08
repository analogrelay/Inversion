using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Storage;
using System.IO;
using Inversion.Utils;

namespace Inversion.Data
{
    public class GitPackFile
    {
        public IFileSystem FileSystem { get; private set; }
        public string PackFileName { get; private set; }
        public ICompressionStrategy Compression { get; private set; }
        public GitPackIndex Index { get; private set; }

        public GitPackFile(IFileSystem fileSystem, string filename, ICompressionStrategy compression, GitPackIndex index)
        {
            FileSystem = fileSystem;
            PackFileName = filename;
            Compression = compression;
            Index = index;
        }

        public static GitPackFile Open(IFileSystem fs, string baseName)
        {
            return new GitPackFile(
                fs,
                baseName + ".pack",
                new ZlibCompressionStrategy(),
                GitPackIndex.Open(fs, baseName + ".idx"));
        }

        public virtual bool Exists(string hash)
        {
            return Index.EntryExists(BitUtils.FromHexString(hash));
        }

        public virtual DatabaseObject GetObject(string hash)
        {
            // Look up the object in the index
            byte[] hashData = BitUtils.FromHexString(hash);
            GitPackIndexEntry entry = Index.GetEntry(hashData);
            if (entry == null)
            {
                return null;
            }

            // Open the pack file and read the object out
            Stream packFile = FileSystem.Open(PackFileName, FileAccess.Read, create: false);
            long size = 0;
            string type = null;

            using (DisposeProtectedStream strm = new DisposeProtectedStream(packFile))
            using (BinaryReader rdr = new BinaryReader(strm))
            {
                strm.Seek(entry.Offset, SeekOrigin.Begin);

                // ==01234567==
                // | 1TTTSSSS |
                // ============
                // T = Type
                // S = Size Start
                byte read = rdr.ReadByte();
                type = InterpretType((read & 0x70) >> 4);
                size = read & 0x0F;

                // Now read until the byte doesn't start with "1"
                // To add the first value, we shift the current value by 4 first
                // But afterwards, shift by 7
                int shiftVal = 4;
                do
                {
                    read = rdr.ReadByte();
                    size += ((read & 0x7F) << shiftVal) /* select low 7 bits only */;
                    shiftVal += 7;
                } while ((read & 0x80) == 0x80);

                if (type == "<<offset-delta>>")
                {
                    read = rdr.ReadByte();
                    long offset = read & 0x7F;
                    while ((read & 0x80) == 0x80)
                    {
                        offset += 1;
                        read = rdr.ReadByte();
                        offset <<= 7;
                        offset += (read & 0x7F);
                    }
                    offset = entry.Offset - offset;
                    type = "<<delta of offset: " + offset.ToString("X") + ">>";
                }
                else if (type == "<<ref-delta>>")
                {
                    type = "<<delta of hash: " + BitConverter.ToString(rdr.ReadBytes(20)).Replace("-", "").ToLower() + ">>";
                }

                byte[] data = Compression.WrapStreamForDecompression(strm)
                                         .ToByteArray();
                return new DatabaseObject(type, data);
            }
        }

        private string InterpretType(int type)
        {
            switch (type)
            {
                case 1:
                    return "commit";
                case 2:
                    return "tree";
                case 3:
                    return "blob";
                case 4:
                    return "tag";
                case 6:
                    return "<<offset-delta>>";
                case 7:
                    return "<<ref-delta>>";
                default:
                    return "<<unknown>>";
            }
        }
    }
}
