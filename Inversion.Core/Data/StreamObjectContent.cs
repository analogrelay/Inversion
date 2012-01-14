using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Inversion.Data
{
    public class StreamObjectContent : IObjectContent
    {
        private Stream _strm;

        public long Length { get; private set; }

        public StreamObjectContent(Stream stream) : this(stream, stream.Length)
        {
        }

        public StreamObjectContent(Stream stream, long length)
        {
            _strm = stream;
            Length = length;
        }

        public Stream OpenRead()
        {
            return _strm;
        }

        public void WriteTo(Stream strm)
        {
            byte[] buffer = new byte[4096];
            int read;
            long totalRead = 0;
            while (totalRead < Length && (read = _strm.Read(buffer, 0, 4096)) > 0)
            {
                // If this read would cause us to overflow
                if(totalRead + read > Length) {
                    // Truncate the amount of data to write
                    Trace.WriteLine(String.Format("[StreamObjectContent.WriteTo]: Had to truncate. Length = \"{0}\", totalRead = \"{1}\"", Length, totalRead));
                    read = (int)(Length - totalRead);
                }
                strm.Write(buffer, 0, read);
            }
        }
    }
}
