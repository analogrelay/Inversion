using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Storage;
using System.IO;

namespace Inversion.Core.Facts.Storage
{
    class InMemoryFileSystem : IFileSystem
    {
        private IDictionary<string, byte[]> _files = new Dictionary<string, byte[]>();

        public string Root { get; private set; }

        public InMemoryFileSystem() : this(String.Empty) { }
        public InMemoryFileSystem(string root)
        {
            Root = root;
        }

        public Stream Open(string relativePath, FileAccess access, bool create)
        {
            MemoryStream strm;
            if (_files.ContainsKey(relativePath))
            {
                // In order to make the "file" expandable, create an empty MemoryStream and write the current data to it.
                byte[] data = _files[relativePath];
                strm = new MemoryStream();
                strm.Write(data, 0, data.Length);
                strm.Seek(0, SeekOrigin.Begin);
            }
            else if (create)
            {
                strm = new MemoryStream();
            }
            else {
                throw new FileNotFoundException(String.Format("File not found: {0}", relativePath));
            }
            // Technically if someone specifies FileAccess.Write, they will get Read permission too, but we don't care for this test class.
            return new CallbackStream<MemoryStream>(strm, callback: UpdateFile(relativePath), writeable: access != FileAccess.Read);
        }

        public bool Exists(string relativePath)
        {
            return _files.ContainsKey(relativePath);
        }

        private Action<MemoryStream> UpdateFile(string relativePath)
        {
            return data =>
            {
                _files[relativePath] = data.ToArray();
            };
        }
    }
}
