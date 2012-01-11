using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Inversion.Storage;
using System.IO;

namespace Inversion.Core.Facts.Storage
{
    public class GitLooseFilesDictionaryFacts
    {
        [Fact]
        public void ConstructorThrowsOnNullFileSystem()
        {
            Assert.Equal("root",
                Assert.Throws<ArgumentNullException>(() => new GitLooseFilesDictionary(null, new NullCompressionStrategy())).ParamName);
        }

        [Fact]
        public void ConstructorThrowsOnNullCompression()
        {
            Assert.Equal("compression",
                Assert.Throws<ArgumentNullException>(() => new GitLooseFilesDictionary(new InMemoryFileSystem(), null)).ParamName);
        }

        [Fact]
        public void ExistsReturnsTrueIfHashFileExists()
        {
            // Arrange
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitLooseFilesDictionary db = new GitLooseFilesDictionary(fs, new NullCompressionStrategy());
            fs.WriteTestFile(@"ab\cdefghijk", "Foo");

            // Act/Assert
            Assert.True(db.Exists("abcdefghijk"));
        }

        [Fact]
        public void ExistsReturnsFalseIfHashFileDoesNotExist()
        {
            // Arrange
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitLooseFilesDictionary db = new GitLooseFilesDictionary(fs, new NullCompressionStrategy());

            // Assume
            Assert.False(fs.Exists(@"ab\cdefghijk"));

            // Act/Assert
            Assert.False(db.Exists("abcdefghijk"));
        }

        [Fact]
        public void OpenExistingKeyForRead()
        {
            // Arrange
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitLooseFilesDictionary db = new GitLooseFilesDictionary(fs, new NullCompressionStrategy());
            fs.WriteTestFile(@"ab\cdefghijk", "Foo");

            // Act
            using (StreamReader reader = new StreamReader(db.OpenRead("abcdefghijk")))
            {
                // Assert
                Assert.Equal("Foo", reader.ReadToEnd());
            }
        }

        [Fact]
        public void OpenExistingKeyForReadDoesNotAllowWrite()
        {
            // Arrange
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitLooseFilesDictionary db = new GitLooseFilesDictionary(fs, new NullCompressionStrategy());
            fs.WriteTestFile(@"ab\cdefghijk", "Foo");

            // Act/Assert
            Assert.Equal("Stream was not writable.",
                Assert.Throws<ArgumentException>(() => new StreamWriter(db.OpenRead("abcdefghijk"))).Message);
        }

        [Fact]
        public void OpenExistingKeyForWriteAndEditData()
        {
            // Arrange
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitLooseFilesDictionary db = new GitLooseFilesDictionary(fs, new NullCompressionStrategy());
            fs.WriteTestFile(@"ab\cdefghijk", "Foo");

            // Act
            using (Stream strm = db.OpenWrite("abcdefghijk", create: false))
            using (StreamWriter writer = new StreamWriter(strm))
            {
                strm.Seek(2, SeekOrigin.Begin);
                writer.Write("Bar");
            }

            // Assert
            Assert.Equal("FoBar", fs.ReadTestFile(@"ab\cdefghijk"));
        }

        [Fact]
        public void OpenNonExistantKeyForReadThrowsKeyNotFound()
        {
            // Arrange
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitLooseFilesDictionary db = new GitLooseFilesDictionary(fs, new NullCompressionStrategy());
            
            // Act/Assert
            Assert.Throws<KeyNotFoundException>(() => new StreamWriter(db.OpenRead("abcdefghijk")));
        }

        [Fact]
        public void OpenNonExistantKeyForWriteWithoutCreateThrowsKeyNotFound()
        {
            // Arrange
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitLooseFilesDictionary db = new GitLooseFilesDictionary(fs, new NullCompressionStrategy());

            // Act/Assert
            Assert.Throws<KeyNotFoundException>(() => new StreamWriter(db.OpenWrite("abcdefghijk", create: false)));
        }
    }
}
