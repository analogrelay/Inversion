using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using Inversion.Utils;

namespace Inversion.Data
{
    public class DatabaseObject : IDisposable
    {
        public string Type { get; private set; }
        public IObjectContent Content { get; private set; }

        public DatabaseObject(string type, IObjectContent content)
        {
            if (String.IsNullOrEmpty(type)) { throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "type"), "type"); }
            if (content == null) { throw new ArgumentNullException("content"); }

            Type = type;
            Content = content;
        }

        ~DatabaseObject()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                IDisposable disposableContent = Content as IDisposable;
                if (disposableContent != null)
                {
                    disposableContent.Dispose();
                }
            }
        }
    }
}
