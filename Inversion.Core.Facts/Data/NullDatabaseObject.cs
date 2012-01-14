using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Data;

namespace Inversion.Core.Facts.Data
{
    class NullDatabaseObject : DatabaseObject
    {
        public NullDatabaseObject() : base("null", new NullObjectContent()) { }
    }
}
