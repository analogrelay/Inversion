using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Inversion.Utils
{
    internal static class BinaryReaderExtensions
    {
        public static uint ReadNetworkUInt32(this BinaryReader self)
        {
            int read = self.ReadInt32();
            int hostOrder = IPAddress.NetworkToHostOrder(read);
            return (uint)hostOrder;
        }

        public static long ReadVarInteger(this BinaryReader self)
        {
            return ReadVarInteger(self, 0).Item2;
        }

        public static Tuple<int, long> ReadVarInteger(this BinaryReader self, byte prefixLength)
        {
            int read = self.ReadByte();

            // Remove the continuation byte and use the rest to start the value counter.
            long value = read & 0x7F /* 0111 1111 */;

            int shiftVal = 7 - prefixLength;
            int prefix = (int)(value >> shiftVal);
            if (prefixLength > 0)
            {
                value = read & (((int)Math.Pow(2, shiftVal) - 1));
            }
            
            while ((read & 0x80 /* 1000 0000 */) == 0x80)
            {
                read = self.ReadByte();
                value += ((read & 0x7F) << shiftVal);
                shiftVal += 7;
            }

            return Tuple.Create(prefix, value);
        }
    }
}
