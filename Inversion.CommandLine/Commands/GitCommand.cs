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
        private string _dbRoot;
        public virtual Database Database
        {
            get
            {
                return _db ?? GetDatabase();
            }
        }

        public virtual string DatabaseRoot
        {
            get
            {
                return _dbRoot ?? GetDatabaseRoot();
            }
        }

        private string GetDatabaseRoot()
        {
            _dbRoot = Git.FindGitDatabase(Environment.CurrentDirectory);
            if (String.IsNullOrEmpty(_dbRoot))
            {
                throw new InvalidOperationException("Not in a Git working copy");
            }
            return _dbRoot;
        }

        private Database GetDatabase()
        {
            return _db = Git.OpenGitDatabase(DatabaseRoot);
        }
    }
}
