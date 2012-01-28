using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Inversion.Storage
{
    public interface IPersistentDictionary
    {
        bool CanRead { get; }
        bool CanWrite { get; }
        bool Exists(string hash);
        Stream OpenRead(string hash);
        Stream OpenWrite(string hash, bool create);
    }
}
