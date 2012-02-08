using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Inversion.Utils
{
    public static class StreamExtensions
    {
        public static byte[] ToByteArray(this Stream strm)
        {
            MemoryStream memStrm = strm as MemoryStream;
            if (memStrm != null)
            {
                return memStrm.ToArray();
            }

            return ReadChunk(strm, 0, strm.Length);
        }

        public static byte[] ReadChunk(this Stream strm, long position, long length)
        {
            strm.Seek(position, SeekOrigin.Begin);
            return ReadChunk(strm, length);
        }
        public static byte[] ReadChunk(this Stream strm, long length)
        {
            const int ChunkSize = 1024;
            byte[] buffer = new byte[length];
            int offset = 0;
            int read = 0;
            while ((read = strm.Read(buffer, offset, Math.Min(ChunkSize, (int)(buffer.LongLength - offset)))) > 0)
            {
                offset += read;
            }
            return buffer;
        }
    }
}
