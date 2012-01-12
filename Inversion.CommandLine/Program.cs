using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Data;
using Inversion.Storage;
using System.IO;

namespace Inversion.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            Database db = new Database(
                new GitLooseFilesDictionary(
                    new PhysicalFileSystem(Path.GetFullPath(@".git\objects")),
                    new ZlibCompressionStrategy()),
                new GitObjectCodec());

            RawDatabaseObject obj = db.GetObject(args[0]) as RawDatabaseObject;
            if (obj == null)
            {
                Console.WriteLine("ERROR: No such object: " + args[0]);
            }
            else
            {
                Console.WriteLine("TYPE: " + obj.Type);
                Console.WriteLine("LEN: " + obj.Length);
                Console.WriteLine("CONTENT:");
                Console.WriteLine(obj.Content);
            }
        }
    }
}
