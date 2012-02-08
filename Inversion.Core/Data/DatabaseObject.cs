using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using Inversion.Utils;

namespace Inversion.Data
{
    public class DatabaseObject
    {
        public DatabaseObjectType Type { get; private set; }
        public byte[] Content { get; private set; }
        public int Length { get { return Content.Length; } }

        public DatabaseObject(DatabaseObjectType type, byte[] content)
        {
            if (content == null) { throw new ArgumentNullException("content"); }

            Type = type;
            Content = content;
        }

        public static DatabaseObject Null()
        {
            return new DatabaseObject(DatabaseObjectType.Null, new byte[0]);
        }
    }
}
