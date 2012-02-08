using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.CommandLine.Infrastructure;
using System.IO;
using Inversion.Data;
using Inversion.Storage;

namespace Inversion.CommandLine.Commands
{
    [Command("dump-index", "Dumps the specified index file to the console", MinArgs = 1, MaxArgs = 1, UsageSummary = "<idxfile>")]
    public class DumpIndexCommand : Command
    {
        public override int ExecuteCommand()
        {
            // Find the index file
            string indexFile = Arguments[0];
            if (!File.Exists(indexFile))
            {
                Console.WriteError("Index file not found: {0}", indexFile);
            }

            // Open the index
            GitPackIndex index = GitPackIndex.Open(access => File.Open(indexFile, FileMode.Open, access));

            // Dump the values
            Console.WriteLine("Dumping Index {0}.", Path.GetFileName(indexFile));
            Console.WriteLine("Version: {0}", index.Version);
            Console.WriteLine();
            Console.WriteLine("Entries:");
            
            GitPackIndexEntry last = null;
            long start = 0;
            foreach (GitPackIndexEntry entry in index.GetEntries().OrderBy(i => i.Offset))
            {
                if (last == null)
                {
                    last = entry;
                    start = entry.Offset;
                }
                else
                {
                    WriteEntry(last, entry);
                    last = entry;
                }
            }
            WriteEntry(last, null);
            return 0;
        }

        private void WriteEntry(GitPackIndexEntry last, GitPackIndexEntry entry)
        {
            Console.WriteLine(
                "\t{0} [{1}-{2}] (Len: {3})", 
                BitConverter.ToString(last.Hash).Replace("-", "").ToLower(), 
                last.Offset.ToString("X10"), 
                entry == null ? "<EOF>" : entry.Offset.ToString("X10"), 
                entry == null ? "<Rest Of File>" : FormatSize(entry.Offset - last.Offset));
        }

        private string FormatSize(long p)
        {
            if (p < 1024)
            {
                return String.Format("{0}B", p);
            }
            else if (p < 1024 * 1024)
            {
                return String.Format("{0}KB", p / 1024);
            }
            else
            {
                return String.Format("{0}MB", p / (1024 * 1024));
            }
        }
    }
}
