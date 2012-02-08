using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Inversion.Data
{
    internal static class BinaryReaderExtensions
    {
        public static uint ReadNetworkUInt32(this BinaryReader self)
        {
            int read = self.ReadInt32();
            int hostOrder = IPAddress.NetworkToHostOrder(read);
            return (uint)hostOrder;
        }
    }
}
