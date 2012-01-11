using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Storage;
using System.IO;

namespace Inversion.Core.Facts.Storage
{
    class NullCompressionStrategy : ICompressionStrategy
    {
        public Stream WrapStreamForDecompression(Stream target)
        {
            return target;
        }

        public Stream WrapStreamForCompression(Stream target)
        {
            return target;
        }
    }
}
