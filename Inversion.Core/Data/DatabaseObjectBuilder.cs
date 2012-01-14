using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inversion.Data
{
    public class DatabaseObjectBuilder
    {
        public string Type { get; set; }
        public IObjectContent Content { get; set; }

        public DatabaseObjectBuilder(string type)
        {
            Type = type;
        }

        public DatabaseObject Build()
        {
            return new DatabaseObject(Type, Content);
        }
    }
}
