using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Data;
using System.IO;

namespace Inversion.Core.Facts.Data
{
    class NullObjectContent : IObjectContent
    {
        public long Length
        {
            get { return 0L; }
        }

        public Stream OpenRead()
        {
            return Stream.Null;
        }

        public void WriteTo(Stream strm)
        {
        }
    }
}
