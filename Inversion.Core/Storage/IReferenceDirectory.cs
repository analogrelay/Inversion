using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inversion.Storage
{
    public interface IReferenceDirectory
    {
        string ResolveReference(string referenceName);
    }
}
