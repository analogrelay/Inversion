using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using Inversion.Utils;

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
            return Root.Exists(FindHash(hash));
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
                    FindHash(hash), 
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
                    FindHash(hash),
                    FileAccess.ReadWrite,
                    create));
        }

        private string FindHash(string partialHash)
        {
            string searchPath;
            if (partialHash.Length == 2)
            {
                searchPath = String.Format(@"{0}\*", partialHash);
            }
            else if (partialHash.Length >= 3)
            {
                searchPath = String.Format(@"{0}\{1}*", partialHash.Substring(0, 2), partialHash.Substring(2));
            }
            else
            {
                return partialHash;
            }
            string[] results = Root.ResolveWildcard(searchPath);
            if (results.Length > 1)
            {
                throw new KeyNotFoundException(String.Format("Prefix '{0}' matches multiple objects in the database.", partialHash));
            }
            if (results.Length == 0)
            {
                if (partialHash.Length < 3)
                {
                    throw new ArgumentException(String.Format("Hash '{0}' does not exist and is not long enough to create a new entry in the database", partialHash), "partialHash");
                }
                return searchPath.TrimEnd('*');
            }
            return results[0];
        }
    }
}
