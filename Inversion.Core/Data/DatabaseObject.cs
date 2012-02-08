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
        public string Type { get; private set; }
        public byte[] Content { get; private set; }
        public int Length { get { return Content.Length; } }

        public DatabaseObject(string type, byte[] content)
        {
            if (String.IsNullOrEmpty(type)) { throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "type"), "type"); }
            if (content == null) { throw new ArgumentNullException("content"); }

            Type = type;
            Content = content;
        }
    }
}
