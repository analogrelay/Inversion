using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Storage;
using System.IO;

namespace Inversion.Data
{
    public class GitPackFileDatabase : IPackedObjectDatabase
    {
        public IFileSystem PackFileRoot { get; private set; }

        private IList<GitPackFile> _files;

        public GitPackFileDatabase(IFileSystem packFileRoot)
        {
            PackFileRoot = packFileRoot;
        }

        public bool Exists(string hash)
        {
            EnsureFiles();
            return _files.Any(f => f.Exists(hash));
        }

        public DatabaseObject GetObject(string hash)
        {
            EnsureFiles();
            return _files.Select(f => f.GetObject(hash))
                         .Where(o => o != null)
                         .FirstOrDefault();
        }

        private void EnsureFiles()
        {
            _files = PackFileRoot.ResolveWildcard("pack-*.idx")
                                 .Select(s => GitPackFile.Open(PackFileRoot, s.Substring(0, s.Length - 4)))
                                 .ToList();
        }
    }
}
