using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Inversion.Data
{
    public interface IObjectCodec
    {
        DatabaseObject Decode(Stream source);
        void Encode(DatabaseObject obj, Stream target);
    }
}
