using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Inversion.Delta
{
    public interface IDeltaDecoder
    {
        void Decode(Stream source, Stream delta, Stream output);
    }
}
