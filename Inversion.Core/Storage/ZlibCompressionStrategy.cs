using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Ionic.Zlib;

namespace Inversion.Storage
{
    public class ZlibCompressionStrategy : ICompressionStrategy
    {
        public Stream WrapStreamForDecompression(Stream target)
        {
            if (target == null) { throw new ArgumentNullException("target"); }
            return new ZlibStream(target, CompressionMode.Decompress, leaveOpen: false);
        }

        public Stream WrapStreamForCompression(Stream target)
        {
            if (target == null) { throw new ArgumentNullException("target"); }
            return new ZlibStream(target, CompressionMode.Compress, leaveOpen: true);
        }
    }
}
