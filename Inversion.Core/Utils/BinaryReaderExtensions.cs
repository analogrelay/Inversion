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

        public static Tuple<int, long> ReadVarInteger(this BinaryReader self, int prefixLength)
        {
            return Tuple.Create(0, 0L);
        }
    }
}
