using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inversion.Data
{
    public interface IPackedObjectDatabase
    {
        bool Exists(string hash);
        DatabaseObject GetObject(string hash);
    }
}
