using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Inversion.Storage
{
    public interface IFileSystem
    {
        string Root { get; }
        Stream Open(string relativePath, FileAccess access, bool create);
        bool Exists(string relativePath);
        string[] ResolveWildcard(string relativeWildCardPath);
    }
}
