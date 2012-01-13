using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Inversion.Utils;
using System.IO;

namespace Inversion.Storage
{
    public class GitReferenceDirectory : IReferenceDirectory
    {
        public IFileSystem Root { get; private set; }

        public GitReferenceDirectory(IFileSystem root)
        {
            if (root == null) { throw new ArgumentNullException("root"); }
            Root = root;
        }

        public string ResolveReference(string referenceName)
        {
            if (String.IsNullOrEmpty(referenceName)) { throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "referenceName"), "referenceName"); }
            
            // Try to find the reference according to gitrevisions man page
            string path = FixPath(referenceName);
            if (Root.Exists(path))
            {
                return ReadReference(path);
            }
            if (Root.Exists(@"refs\" + path))
            {
                return ReadReference(@"refs\" + path);
            }
            if (Root.Exists(@"refs\tags\" + path))
            {
                return ReadReference(@"refs\tags\" + path);
            }
            if (Root.Exists(@"refs\heads\" + path))
            {
                return ReadReference(@"refs\heads\" + path);
            }
            if (Root.Exists(@"refs\remotes\" + path))
            {
                return ReadReference(@"refs\remotes\" + path);
            }
            if (Root.Exists(@"refs\remotes\" + path + @"\HEAD"))
            {
                return ReadReference(@"refs\remotes\" + path + @"\HEAD");
            }
            
            // No match, it's not a reference name
            return referenceName;
        }

        private string ReadReference(string referenceName)
        {
            // Read the content of the file
            string content;
            using(StreamReader sr = new StreamReader(Root.Open(referenceName, FileAccess.Read, create: false))) {
                content = sr.ReadToEnd();
            }

            // Check for a "ref" prefix
            if(content.StartsWith("ref:", StringComparison.OrdinalIgnoreCase)) {
                return ReadReference(FixPath(content.Substring(4).Trim()));
            }
            return content.Trim();
        }

        private string FixPath(string path)
        {
            return path.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
