using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inversion.Utils;

namespace Inversion.Delta
{
    internal class VcdiffDecoderInstruction
    {
        public VcdiffDecoderContext Context { get; set; }
        public VcdiffOperation Operation { get; set; }
        public long Size { get; set; }
        public VcdiffCopyMode CopyMode { get; set; }

        public void Apply(Stream source, Stream output, VcdiffAddressCache cache)
        {
            byte[] outputData = null;
            switch (Operation)
            {
                case VcdiffOperation.Add:
                    // Read [Size] bytes from [Data] and write them to the [output]
                    outputData = Context.Data.ReadChunk(Size);
                    break;
                case VcdiffOperation.Run:
                    // Read one byte from [Data] and write it [Size] times
                    int b = Context.Data.ReadByte();
                    if (b == -1)
                    {
                        throw new InvalidDataException("Reached end of Data Stream while trying to read byte for RUN instruction");
                    }
                    outputData = new byte[Size];
                    for (int i = 0; i < Size; i++)
                    {
                        outputData[i] = (byte)b;
                    }
                    break;
                case VcdiffOperation.Copy:
                    outputData = GetCopySource(source, output, cache);
                    break;
            }

            if (outputData != null)
            {
                output.Write(outputData, 0, outputData.Length);
            }
        }

        private byte[] GetCopySource(Stream source, Stream output, VcdiffAddressCache cache)
        {
            long address;
            switch (CopyMode.Type)
            {
                case VcdiffCopyModeType.Self:
                    address = Context.Addresses.ReadVarInteger();
                    break;
                case VcdiffCopyModeType.Here:
                    address = output.Position - Context.Addresses.ReadVarInteger();
                    break;
                case VcdiffCopyModeType.Near:
                    address = Context.Addresses.ReadVarInteger() - cache.GetNear(CopyMode.Index);
                    break;
                case VcdiffCopyModeType.Same:
                    byte index = Context.Addresses.ReadByte();
                    address = cache.GetSame(CopyMode.Index, index);
                    break;
                default:
                    throw new InvalidDataException("Unknown Copy Mode: " + CopyMode.Type.ToString());
            }
            cache.Update(address);
            return source.ReadChunk(address, Size);
        }
    }
}
