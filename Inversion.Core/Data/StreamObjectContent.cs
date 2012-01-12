using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Inversion.Data
{
    public class StreamObjectContent : IObjectContent
    {
        private Stream _strm;

        public StreamObjectContent(Stream stream)
        {
            _strm = stream;
        }

        public Stream OpenRead()
        {
            return _strm;
        }

        public void WriteTo(Stream strm)
        {
            _strm.CopyTo(strm);
        }
    }
}
