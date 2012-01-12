using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.CommandLine.Infrastructure;
using Inversion.Data;
using Inversion.Storage;
using System.IO;

namespace Inversion.CommandLine.Commands
{
    [Command("cat-file", "Displays the specified database file content", UsageSummary = "<object> [options]", MinArgs = 1)]
    public class CatFileCommand : Command
    {
        [Option("Shows the type of the object", AltName = "t")]
        public bool Type { get; set; }

        [Option("Shows the size of the object", AltName = "s")]
        public bool Size { get; set; }

        [Option("Shows the content of the object", AltName = "p")]
        public bool Content { get; set; }

        public override void ExecuteCommand()
        {
            // Find the object database
            string dbRoot = Path.Combine(Environment.CurrentDirectory, @".git\objects");
            if(!Directory.Exists(dbRoot))
            {
                Console.WriteError("Not a git working directory root: {0}", Environment.CurrentDirectory);
                return;
            }

            Database db = new Database(
                new GitLooseFilesDictionary(
                    new PhysicalFileSystem(dbRoot),
                    new ZlibCompressionStrategy()),
                new GitObjectCodec());

            DatabaseObject obj = db.GetObject(Arguments[0]);
            if (obj == null)
            {
                Console.WriteError("No such object: {0}", Arguments[0]);
            }
            else
            {
                if (Type)
                {
                    Console.WriteLine(obj.Type);
                }
                else if (Size)
                {
                    Console.WriteLine(obj.Length);
                }
                else if (Content)
                {
                    using (StreamReader reader = new StreamReader(obj.Content.OpenRead()))
                    {
                        Console.WriteLine(reader.ReadToEnd());
                    }
                }
            }
        }
    }
}
