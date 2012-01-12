using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Inversion.Storage
{
    public class GitLooseFilesDictionary : IPersistentDictionary
    {
        public IFileSystem Root { get; set; }
        public ICompressionStrategy Compression { get; set; }

        public GitLooseFilesDictionary(IFileSystem root, ICompressionStrategy compression)
        {
            if (root == null) { throw new ArgumentNullException("root"); }
            if (compression == null) { throw new ArgumentNullException("compression"); }

            Root = root;
            Compression = compression;
        }

        public bool Exists(string hash)
        {
            if (String.IsNullOrEmpty(hash)) { throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "hash"), "hash"); }
            return Root.Exists(HashToPath(hash));
        }

        public Stream OpenRead(string hash)
        {
            if (String.IsNullOrEmpty(hash)) { throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "hash"), "hash"); }
            if (!Exists(hash))
            {
                throw new KeyNotFoundException(String.Format("Hash '{0}' does not exist in the database.", hash));
            }
            return Compression.WrapStreamForDecompression(
                Root.Open(
                    HashToPath(hash), 
                    FileAccess.Read, 
                    create: false));
        }

        public Stream OpenWrite(string hash, bool create)
        {
            if (String.IsNullOrEmpty(hash)) { throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "hash"), "hash"); }
            if (!Exists(hash) && !create)
            {
                throw new KeyNotFoundException(String.Format("Hash '{0}' does not exist in the database.", hash));
            }
            return Compression.WrapStreamForCompression(
                Root.Open(
                    HashToPath(hash),
                    FileAccess.ReadWrite,
                    create));
        }

        private string HashToPath(string hash)
        {
            if (hash.Length < 3)
            {
                return String.Format(@"_\{0}", hash);
            }
            else
            {
                return String.Format(@"{0}\{1}", hash.Substring(0, 2), hash.Substring(2));
            }
        }
    }
}
