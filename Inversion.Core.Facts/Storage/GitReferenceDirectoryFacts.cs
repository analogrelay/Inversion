using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Inversion.Storage;
using Xunit.Extensions;

namespace Inversion.Core.Facts.Storage
{
    public class GitReferenceDirectoryFacts
    {
        [Fact]
        public void ConstructorRequiresNonNullFileSystem()
        {
            Assert.Throws<ArgumentNullException>(() => new GitReferenceDirectory(null))
                  .WithParamName("root");
        }

        [Fact]
        public void ResolveReferenceRequiresNonNullOrEmptyName()
        {
            GitReferenceDirectory dir = new GitReferenceDirectory(new InMemoryFileSystem());
            Assert.Throws<ArgumentException>(() => dir.ResolveReference(null))
                  .WithParamName("referenceName");
            Assert.Throws<ArgumentException>(() => dir.ResolveReference(String.Empty))
                  .WithParamName("referenceName");
        }

        [Theory]
        [InlineData("HEAD", "HEAD")]
        [InlineData("foo", @"refs\foo")]
        [InlineData("1.0", @"refs\tags\1.0")]
        [InlineData("master", @"refs\heads\master")]
        [InlineData("origin", @"refs\remotes\origin")]
        [InlineData("origin", @"refs\remotes\origin\HEAD")]
        public void ResolveReferenceSingleIndirection(string name, string actualFile)
        {
            // Arrange
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitReferenceDirectory refs = new GitReferenceDirectory(fs);

            fs.WriteTestFile(actualFile, "abcdefghij");

            // Act/Assert
            Assert.Equal("abcdefghij", refs.ResolveReference(name));
        }

        [Theory]
        [InlineData("HEAD", "HEAD")]
        [InlineData("foo", @"refs\foo")]
        [InlineData("1.0", @"refs\tags\1.0")]
        [InlineData("master", @"refs\heads\master")]
        [InlineData("origin", @"refs\remotes\origin")]
        [InlineData("origin", @"refs\remotes\origin\HEAD")]
        public void ResolveReferenceDoubleIndirection(string name, string actualFile)
        {
            // Arrange
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitReferenceDirectory refs = new GitReferenceDirectory(fs);

            fs.WriteTestFile(actualFile, "ref: refs/remotes/origin/master");
            fs.WriteTestFile(@"refs\remotes\origin\master", "abcdefghij");

            // Act/Assert
            Assert.Equal("abcdefghij", refs.ResolveReference(name));
        }

        [Fact]
        public void ResolveReferenceWithHash()
        {
            // Arrange
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitReferenceDirectory refs = new GitReferenceDirectory(fs);

            // Assume
            Assert.False(fs.Exists("abcdefghij"));

            // Act/Assert
            Assert.Equal("abcdefghij", refs.ResolveReference("abcdefghij"));
        }
    }
}
