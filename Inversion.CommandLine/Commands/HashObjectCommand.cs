using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.CommandLine.Infrastructure;
using Inversion.Data;
using System.IO;

namespace Inversion.CommandLine.Commands
{
    [Command("hash-object", "Computes the Object ID for the object contained in the specified file", MinArgs = 1)]
    public class HashObjectCommand : GitCommand
    {
        [Option("The type of the object, defaults to blob", AltName="t")]
        public string Type { get; set; }

        public override void ExecuteCommand()
        {
            // Create the object
            DatabaseObjectBuilder builder = new DatabaseObjectBuilder(Type ?? "blob");

            // Open the file and make it the object content
            using (Stream strm = new FileStream(Arguments[0], FileMode.Open, FileAccess.Read))
            {
                builder.Content = new StreamObjectContent(strm);
                DatabaseObject obj = builder.Build();
                Database.StoreObject(obj);
            }
        }
    }
}
