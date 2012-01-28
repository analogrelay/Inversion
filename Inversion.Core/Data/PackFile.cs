using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Storage;
using System.IO;

namespace Inversion.Data
{
    public class PackFile
    {
        public IFileSystem FileSystem { get; private set; }
        public string PackFileName { get; private set; }
        public ICompressionStrategy Compression { get; private set; }
        public PackIndex Index { get; private set; }

        public PackFile(IFileSystem fileSystem, string filename, ICompressionStrategy compression, PackIndex index)
        {
            FileSystem = fileSystem;
            PackFileName = filename;
            Compression = compression;
            Index = index;
        }

        public virtual bool Exists(byte[] hash)
        {
            return Index.EntryExists(hash);
        }

        public virtual DatabaseObject GetObject(byte[] hash)
        {
            // Look up the object in the index
            PackIndexEntry entry = Index.GetEntry(hash);
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
                type = InterpretType(read & 0x70);
                size = read & 0x0F;

                // Now read until the byte doesn't start with "1"
                // To add the first value, we shift the current value by 4 first
                // But afterwards, shift by 7
                int shiftVal = 4;
                do
                {
                    read = rdr.ReadByte();
                    size = (size << shiftVal) + (read & 0x7F) /* select low 7 bits only */;
                    shiftVal = 7;
                } while ((read & 0x80) != 0x80);
            }
            return null;
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
                default:
                    return "unknown";
            }
        }
    }
}
