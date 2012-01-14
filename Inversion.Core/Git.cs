using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Data;
using Inversion.Storage;
using System.IO;
using System.Security.Cryptography;

namespace Inversion
{
    public static class Git
    {
        public static string FindGitDatabase(string currentPath)
        {
            DirectoryInfo curDir = new DirectoryInfo(currentPath);
            if (curDir.Name.Equals(".git", StringComparison.Ordinal))
            {
                return currentPath;
            }
            DirectoryInfo gitDir = null;
            while (curDir != null && (gitDir = curDir.GetDirectories(".git").FirstOrDefault()) == null)
            {
                curDir = curDir.Parent;
            }
            if (curDir != null && gitDir != null)
            {
                return gitDir.FullName;
            }
            return null;
        }

        public static Database OpenGitDatabase(string gitDir)
        {
            return new Database(
                new HashGenerator(new SHA1Managed()),
                new GitReferenceDirectory(new PhysicalFileSystem(gitDir)),
                new GitLooseFilesDictionary(
                    new PhysicalFileSystem(Path.Combine(gitDir, "objects")),
                    new ZlibCompressionStrategy()
                ),
                new GitObjectCodec());
        }
    }
}
