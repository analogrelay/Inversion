using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Storage;
using System.IO;
using Inversion.Utils;
using System.Diagnostics;
using Inversion.Delta;
using System.Globalization;

namespace Inversion.Data
{
    public class GitPackFile
    {
        public IFileSystem FileSystem { get; private set; }
        public string PackFileName { get; private set; }
        public ICompressionStrategy Compression { get; private set; }
        public GitPackIndex Index { get; private set; }
        public IDeltaDecoder Delta { get; private set; }

        public GitPackFile(IFileSystem fileSystem, string filename, ICompressionStrategy compression, GitPackIndex index, IDeltaDecoder delta)
        {
            if (fileSystem == null) { throw new ArgumentNullException("fileSystem"); }
            if (String.IsNullOrEmpty(filename)) { throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "filename"), "filename"); }
            if (compression == null) { throw new ArgumentNullException("compression"); }
            if (index == null) { throw new ArgumentNullException("index"); }
            if (delta == null) { throw new ArgumentNullException("delta"); }

            FileSystem = fileSystem;
            PackFileName = filename;
            Compression = compression;
            Index = index;
            Delta = delta;
        }

        public static GitPackFile Open(IFileSystem fs, string baseName)
        {
            return new GitPackFile(
                fs,
                baseName + ".pack",
                new ZlibCompressionStrategy(),
                GitPackIndex.Open(fs, baseName + ".idx"),
                new GitDeltaDecoder());
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
            return GetObjectCore(entry.Offset, packFile, recursing: false);
        }

        private DatabaseObject GetObjectCore(long objectOffset, Stream packFile, bool recursing)
        {
            long size = 0;
            DatabaseObjectType type = DatabaseObjectType.Null;

            using (BinaryReader rdr = new BinaryReader(packFile, Encoding.UTF8, leaveOpen: true))
            {
                packFile.Seek(objectOffset, SeekOrigin.Begin);

                // ==01234567==
                // | 1TTTSSSS |
                // ============
                // T = Type
                // S = Size Start
                var sizeAndType = rdr.ReadVarInteger(3);
                type = InterpretType(sizeAndType.Item1);
                size = sizeAndType.Item2;

                long deltaOffset = -1;
                if (type == DatabaseObjectType.OffsetDelta)
                {
                    // ReadVarInteger doesn't do the right thing here...
                    byte read = rdr.ReadByte();
                    long offset = read & 0x7F;
                    while ((read & 0x80) == 0x80)
                    {
                        offset += 1;
                        read = rdr.ReadByte();
                        offset <<= 7;
                        offset += (read & 0x7F);
                    }
                    deltaOffset = objectOffset - offset;
                }
                else if (type == DatabaseObjectType.HashDelta)
                {
                    byte[] hash = rdr.ReadBytes(20);
                    GitPackIndexEntry entry = Index.GetEntry(hash);
                    deltaOffset = entry.Offset;
                }
                
                byte[] data = Compression.WrapStreamForDecompression(packFile)
                                         .ReadBytes(size);

                if (deltaOffset >= 0)
                {
                    DatabaseObject source = GetObjectCore(deltaOffset, packFile, recursing: true);
                    return ConstructFromDelta(source, data);
                }
                else
                {
                    return new DatabaseObject(type, data);
                }
            }
        }

        private DatabaseObject ConstructFromDelta(DatabaseObject source, byte[] deltaData)
        {
            byte[] data;
            using (MemoryStream output = new MemoryStream())
            using (MemoryStream input = new MemoryStream(source.Content))
            using (MemoryStream delta = new MemoryStream(deltaData))
            {
                Delta.Decode(input, delta, output);
                output.Flush();
                data = output.ToArray();
            }
            return new DatabaseObject(source.Type, data);
        }

        private DatabaseObjectType InterpretType(int type)
        {
            switch (type)
            {
                case 1: return DatabaseObjectType.Commit;
                case 2: return DatabaseObjectType.Tree;
                case 3: return DatabaseObjectType.Blob;
                case 4: return DatabaseObjectType.Tag;
                case 6: return DatabaseObjectType.OffsetDelta;
                case 7: return DatabaseObjectType.HashDelta;
                default: return DatabaseObjectType.Null;
            }
        }
    }
}
