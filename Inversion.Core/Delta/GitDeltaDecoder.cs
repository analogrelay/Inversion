using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inversion.Utils;

namespace Inversion.Delta
{
    public class GitDeltaDecoder : IDeltaDecoder
    {
        const byte COPY = 0x80;

        public void Decode(Stream source, Stream delta, Stream output)
        {
            using (BinaryReader deltaReader = new BinaryReader(delta))
            {
                long baseLength = deltaReader.ReadVarInteger();
                Debug.Assert(baseLength == source.Length);

                long resultLength = deltaReader.ReadVarInteger();
                output.SetLength(resultLength);

                while (delta.Position < delta.Length)
                {
                    byte cmd = deltaReader.ReadByte();
                    if ((cmd & COPY) != 0x00)
                    {
                        DoCopy(source, output, deltaReader, cmd);
                    }
                    else if(cmd != 0)
                    {
                        // 0 is reserved, but anything else is a length of data from the delta itself
                        byte[] data = deltaReader.ReadBytes(cmd);
                        output.Write(data, 0, data.Length);
                    }
                }
            }
            output.Flush();
        }

        private static void DoCopy(Stream source, Stream output, BinaryReader deltaReader, byte cmd)
        {
            // Copy command, read offset and size
            // ==========
            // |76543210|
            // ==========
            // Each bit from 0-3 indicates another byte of data in the offset
            int offset = 0;
            if ((cmd & 0x01) != 0)
            {
                offset += deltaReader.ReadByte();
            }
            if ((cmd & 0x02) != 0)
            {
                offset += deltaReader.ReadByte() << 8;
            }
            if ((cmd & 0x04) != 0)
            {
                offset += deltaReader.ReadByte() << 16;
            }
            if ((cmd & 0x08) != 0)
            {
                offset += deltaReader.ReadByte() << 24;
            }

            // Ditto for size, except it's bits 4-6
            int size = 0;
            if ((cmd & 0x10) != 0)
            {
                size += deltaReader.ReadByte();
            }
            if ((cmd & 0x20) != 0)
            {
                size += deltaReader.ReadByte() << 8;
            }
            if ((cmd & 0x40) != 0)
            {
                size += deltaReader.ReadByte() << 16;
            }

            // However, if size is 0, it's actually 65536
            if (size == 0)
            {
                size = 0x10000;
            }

            // Now do the copy
            byte[] data = source.ReadChunk(offset, size);
            output.Write(data, 0, data.Length);
        }
    }
}
