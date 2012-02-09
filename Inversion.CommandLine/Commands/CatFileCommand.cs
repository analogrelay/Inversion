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
    [Command("cat-file", "Displays the specified database file content", UsageSummary = "<object> [options]", MinArgs = 1, MaxArgs = 1)]
    public class CatFileCommand : GitCommand
    {
        [Option("Shows the type of the object", AltName = "t")]
        public bool Type { get; set; }

        [Option("Shows the size of the object", AltName = "s")]
        public bool Size { get; set; }
        
        [Option("Shows the content of the object", AltName = "p")]
        public bool Content { get; set; }

        [Option("An optional file to output the data to, instead of writing it to the console", AltName = "o")]
        public string Output { get; set; }

        public override int ExecuteCommand()
        {
            // Find the object database
            DatabaseObject obj = Database.GetObject(Database.ResolveReference(Arguments[0]));
            if (obj == null)
            {
                Console.WriteError("No such object: {0}", Arguments[0]);
            }
            else
            {
                if (Type)
                {
                    Console.WriteLine(obj.Type.ToString().ToLowerInvariant());
                }
                else if (Size)
                {
                    Console.WriteLine(obj.Content.Length);
                }
                else if (Content)
                {
                    if (!String.IsNullOrEmpty(Output))
                    {
                        File.WriteAllBytes(Output, obj.Content);
                    }
                    else
                    {
                        using (StreamReader reader = new StreamReader(new MemoryStream(obj.Content)))
                        {
                            Console.WriteLine(reader.ReadToEnd().Trim());
                        }
                    }
                }
            }
            return 0;
        }
    }
}
