using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.CommandLine.Infrastructure;
using Inversion.Data;
using System.IO;

namespace Inversion.CommandLine.Commands
{
    [Command("hash-object", "Computes the Object ID for the object contained in the specified file", MinArgs = 1,
        UsageSummary = "<file> [options]")]
    public class HashObjectCommand : GitCommand
    {
        [Option("The type of the object, defaults to blob", AltName="t")]
        public string Type { get; set; }

        [Option("Specify this option to store the object, instead of just showing the hash", AltName = "w")]
        public bool Store { get; set; }

        public override void ExecuteCommand()
        {
            // Open the file and make it the object content
            using (Stream strm = new FileStream(Arguments[0], FileMode.Open, FileAccess.Read))
            {
                // Create the object
                DatabaseObject obj = new DatabaseObject(Type ?? "blob", new StreamObjectContent(strm));
                if (Store)
                {
                    Console.WriteLine(Database.StoreObject(obj));
                }
                else {
                    Console.WriteLine(Database.ComputeHash(obj));
                }
            }
        }
    }
}
