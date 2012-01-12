using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.CommandLine.Infrastructure;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Inversion.CommandLine
{
    public interface ICommandManager
    {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Method would do reflection and a property would be inappropriate.")]
        IEnumerable<ICommand> GetCommands();
        ICommand GetCommand(string commandName);
        IDictionary<OptionAttribute, PropertyInfo> GetCommandOptions(ICommand command);
        void RegisterCommand(ICommand command);
    }
}
