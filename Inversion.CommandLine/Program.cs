using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Data;
using Inversion.Storage;
using System.IO;
using System.ComponentModel.Composition;
using Inversion.CommandLine.Commands;
using Inversion.CommandLine.Infrastructure;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;

namespace Inversion.CommandLine
{
    public class Program
    {
        [Import]
        public HelpCommand HelpCommand { get; set; }

        [ImportMany]
        public IEnumerable<ICommand> Commands { get; set; }

        [Import]
        public ICommandManager Manager { get; set; }

        public static int Main(string[] args)
        {
            var console = new Inversion.CommandLine.Infrastructure.Console();
            var p = new Program();
            p.Initialize();

            // Add commands to the manager
            foreach (ICommand cmd in p.Commands)
            {
                p.Manager.RegisterCommand(cmd);
            }

            CommandLineParser parser = new CommandLineParser(p.Manager);

            // Parse the command
            ICommand command = parser.ParseCommandLine(args) ?? p.HelpCommand;

            // Fallback on the help command if we failed to parse a valid command
            if (!ArgumentCountValid(command))
            {
                // Get the command name and add it to the argument list of the help command
                string commandName = command.CommandAttribute.CommandName;

                // Print invalid command then show help
                console.WriteLine(InversionResources.InvalidArguments, commandName);

                p.HelpCommand.ViewHelpForCommand(commandName);
                return 0;
            }
            else
            {
                return command.Execute();
            }
        }

        private static bool ArgumentCountValid(ICommand command)
        {
            CommandAttribute attribute = command.CommandAttribute;
            return command.Arguments.Count >= attribute.MinArgs &&
                   command.Arguments.Count <= attribute.MaxArgs;
        }

        private void Initialize()
        {
            using (var catalog = new AggregateCatalog(new AssemblyCatalog(GetType().Assembly)))
            {
                using (var container = new CompositionContainer(catalog))
                {
                    container.ComposeParts(this);
                }
            }
        }
    }
}
