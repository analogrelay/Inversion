using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Inversion.Data
{
    public interface IObjectContent
    {
        Stream OpenRead();
        void WriteTo(Stream strm);
    }
}
