using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Inversion.Utils;

namespace Inversion.Delta
{
    // http://tools.ietf.org/html/rfc3284
    public class VcdiffDecoder : IDeltaDecoder
    {
        private VcdiffCodeTable _codeTable = VcdiffCodeTable.Default;

        public void Decode(Stream source, Stream delta, Stream output)
        {
            VcdiffAddressCache cache = new VcdiffAddressCache(VcdiffConstants.S_NEAR, VcdiffConstants.S_SAME);
            using(BinaryReader deltaReader = new BinaryReader(delta))
            {
                // Verify the header
                if (!deltaReader.ReadBytes(4).SequenceEqual(VcdiffConstants.V0Header))
                {
                    throw new InvalidDataException("Delta is not a VCDiff v0 delta");
                }

                // Check the decompress and code table indicators
                byte indicator = deltaReader.ReadByte();
                if (BitUtils.IsSet(indicator, VcdiffConstants.VCD_DECOMPRESS))
                {
                    throw new NotSupportedException("Secondary decompressors are not supported in this decoder");
                }
                if (BitUtils.IsSet(indicator, VcdiffConstants.VCD_CODETABLE))
                {
                    throw new NotSupportedException("Application-specific code tables are not supported in this decoder");
                }

                // Read windows
                while (deltaReader.BaseStream.Position < deltaReader.BaseStream.Length) {
                    ReadWindow(source, deltaReader, output, cache);
                }
            }
        }

        private void ReadWindow(Stream source, BinaryReader deltaReader, Stream output, VcdiffAddressCache cache)
        {
            byte[] sourceWindow = ExtractSourceWindow(source, deltaReader, output);
            long deltaSize = deltaReader.ReadVarInteger();
            long targetSize = deltaReader.ReadVarInteger();
            
            byte indicator = deltaReader.ReadByte();
            if (indicator != 0x00)
            {
                throw new NotSupportedException("Secondary decompressors are not supported in this decoder");
            }

            VcdiffDecoderContext context = LoadContext(deltaReader);
            VcdiffDecoderInstruction inst;
            while ((inst = context.GetNextOperation()) != null)
            {
                inst.Apply(source, output, cache);
            }
        }

        private VcdiffDecoderContext LoadContext(BinaryReader deltaReader)
        {
            long dataLen = deltaReader.ReadVarInteger();
            long instLen = deltaReader.ReadVarInteger();
            long addrLen = deltaReader.ReadVarInteger();

            byte[] data = deltaReader.BaseStream.ReadChunk(dataLen);
            byte[] inst = deltaReader.BaseStream.ReadChunk(instLen);
            byte[] addr = deltaReader.BaseStream.ReadChunk(addrLen);

            return new VcdiffDecoderContext()
            {
                Data = new MemoryStream(data),
                Instructions = new BinaryReader(new MemoryStream(inst)),
                Addresses = new BinaryReader(new MemoryStream(addr)),
                CodeTable = _codeTable
            };
        }

        private byte[] ExtractSourceWindow(Stream source, BinaryReader deltaReader, Stream output)
        {
            // Decode the Window Origin
            WindowOrigin origin = WindowOrigin.None;
            byte indicator = deltaReader.ReadByte();
            if (BitUtils.IsSet(indicator, VcdiffConstants.VCD_SOURCE))
            {
                origin = WindowOrigin.Source;
            }
            else if (BitUtils.IsSet(indicator, VcdiffConstants.VCD_TARGET))
            {
                origin = WindowOrigin.Target;
            }

            long length = 0;
            long position = 0;
            if (origin != WindowOrigin.None)
            {
                length = deltaReader.ReadVarInteger();
                position = deltaReader.ReadVarInteger();
            }

            switch (origin)
            {
                case WindowOrigin.Source: return source.ReadChunk(position, length);
                case WindowOrigin.Target: return output.ReadChunk(position, length);
                default: return new byte[0];
            }
        }

        private enum WindowOrigin
        {
            Source,
            Target,
            None
        }
    }
}
