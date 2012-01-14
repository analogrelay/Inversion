using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.CommandLine.Infrastructure;
using Inversion.Data;

namespace Inversion.CommandLine.Commands
{
    public abstract class GitCommand : Command
    {
        private Database _db;
        public virtual Database Database
        {
            get
            {
                return _db ?? GetDatabase();
            }
        }

        private Database GetDatabase()
        {
            string dbRoot = Git.FindGitDatabase(Environment.CurrentDirectory);
            if (String.IsNullOrEmpty(dbRoot))
            {
                throw new InvalidOperationException("Not in a Git working copy");
            }
            return _db = Git.OpenGitDatabase(dbRoot);
        }
    }
}
