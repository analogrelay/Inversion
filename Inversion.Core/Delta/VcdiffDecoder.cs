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
                byte[] windowBuffer;
                while (ReadWindow(source, deltaReader, output, out windowBuffer)) { }
            }
        }

        private bool ReadWindow(Stream source, BinaryReader deltaReader, Stream output, out byte[] outputWindow)
        {
            byte[] sourceWindow = ExtractSourceWindow(source, deltaReader, output);
            long deltaSize = deltaReader.ReadVarInteger();
            long targetSize = deltaReader.ReadVarInteger();
            outputWindow = new byte[targetSize];

            byte indicator = deltaReader.ReadByte();
            if (indicator != 0x00)
            {
                throw new NotSupportedException("Secondary Compression is not currently supported");
            }

            long dataLen = deltaReader.ReadVarInteger();
            long instLen = deltaReader.ReadVarInteger();
            long addrLen = deltaReader.ReadVarInteger();

            byte[] data = deltaReader.BaseStream.ReadChunk(dataLen);
            byte[] inst = deltaReader.BaseStream.ReadChunk(instLen);
            byte[] addr = deltaReader.BaseStream.ReadChunk(addrLen);

            long dataP = 0;
            long instP = 0;
            long addrP = 0;

            while (instP < instLen)
            {
                ProcessInstruction(outputWindow, sourceWindow, data, inst, addr, ref dataP, ref instP, ref addrP);
            }
        }

        private void ProcessInstruction(byte[] outputWindow, byte[] sourceWindow, byte[] data, byte[] inst, byte[] addr, ref long dataP, ref long instP, ref long addrP)
        {
            
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
