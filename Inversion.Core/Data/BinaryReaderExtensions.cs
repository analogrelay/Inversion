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
        public static int ReadNetworkInt32(this BinaryReader self)
        {
            return IPAddress.NetworkToHostOrder(self.ReadInt32());
        }
    }
}
