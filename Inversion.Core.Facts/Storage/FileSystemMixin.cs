using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Storage;
using System.IO;

namespace Inversion.Core.Facts.Storage
{
    public static class FileSystemMixin
    {
        public static void WriteTestFile(this IFileSystem fs, string name, string content)
        {
            using (Stream s = fs.Open(name, FileAccess.Write, create: true))
            {
                using (StreamWriter sw = new StreamWriter(s))
                {
                    sw.Write(content);
                }
            }
        }

        public static string ReadTestFile(this IFileSystem fs, string name)
        {
            using (Stream s = fs.Open(name, FileAccess.Read, create: false))
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
