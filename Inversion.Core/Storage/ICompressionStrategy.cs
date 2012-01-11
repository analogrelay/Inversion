using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Inversion.Storage
{
    public interface ICompressionStrategy
    {
        Stream WrapStreamForDecompression(Stream target);
        Stream WrapStreamForCompression(Stream target);
    }
}
