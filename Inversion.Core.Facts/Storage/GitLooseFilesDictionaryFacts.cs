using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Inversion.Storage;
using System.IO;
using Moq;

namespace Inversion.Core.Facts.Storage
{
    public class GitLooseFilesDictionaryFacts
    {
        [Fact]
        public void ConstructorThrowsOnNullFileSystem()
        {
            Assert.Throws<ArgumentNullException>(() => new GitLooseFilesDictionary(null, new NullCompressionStrategy()))
                  .WithParamName("root");
        }

        [Fact]
        public void ConstructorThrowsOnNullCompression()
        {
            Assert.Throws<ArgumentNullException>(() => new GitLooseFilesDictionary(new InMemoryFileSystem(), null))
                  .WithParamName("compression");
        }

        [Fact]
        public void ExistsThrowsOnNullOrEmptyHash()
        {
            GitLooseFilesDictionary dict = new GitLooseFilesDictionary(new InMemoryFileSystem(), new NullCompressionStrategy());
            Assert.Throws<ArgumentException>(() => dict.Exists(null))
                  .WithParamName("hash");
            Assert.Throws<ArgumentException>(() => dict.Exists(String.Empty))
                  .WithParamName("hash");
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
        public void OpenReadThrowsOnNullOrEmptyHash()
        {
            GitLooseFilesDictionary dict = new GitLooseFilesDictionary(new InMemoryFileSystem(), new NullCompressionStrategy());
            Assert.Throws<ArgumentException>(() => dict.OpenRead(null))
                  .WithParamName("hash");
            Assert.Throws<ArgumentException>(() => dict.OpenRead(String.Empty))
                  .WithParamName("hash");
        }

        [Fact]
        public void OpenWriteThrowsOnNullOrEmptyHash()
        {
            GitLooseFilesDictionary dict = new GitLooseFilesDictionary(new InMemoryFileSystem(), new NullCompressionStrategy());
            Assert.Throws<ArgumentException>(() => dict.OpenWrite(null, create: false))
                  .WithParamName("hash");
            Assert.Throws<ArgumentException>(() => dict.OpenWrite(String.Empty, create: false))
                  .WithParamName("hash");
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

        [Fact]
        public void OpenNonExistantKeyForWriteWithCreateCreatesNewData()
        {
            // Arrange
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitLooseFilesDictionary db = new GitLooseFilesDictionary(fs, new NullCompressionStrategy());

            // Assume
            Assert.False(fs.Exists(@"ab\cdefghijk"));

            // Act
            using (Stream strm = db.OpenWrite("abcdefghijk", create: true))
            using(StreamWriter writer = new StreamWriter(strm))
            {
                writer.Write("FooBarBaz");
            }

            // Assert
            Assert.Equal("FooBarBaz", fs.ReadTestFile(@"ab\cdefghijk"));
        }

        [Fact]
        public void HashesLessThan3CharactersThrowWhenNeededToCreateObjects()
        {
            // Arrange
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitLooseFilesDictionary db = new GitLooseFilesDictionary(fs, new NullCompressionStrategy());
            fs.WriteTestFile(@"xy", "Foo");

            // Act
            Assert.Equal(
                "Hash 'xy' does not exist and is not long enough to create a new entry in the database\r\nParameter name: partialHash",
                Assert.Throws<ArgumentException>(() => db.OpenWrite("xy", create: true)).Message);
        }

        [Fact]
        public void OpenReadUsesCompressionStrategyToWrapStreamForDecompression()
        {
            // Arrange
            Mock<ICompressionStrategy> mockStrategy = new Mock<ICompressionStrategy>(MockBehavior.Strict);
            Stream expected = new MemoryStream();
            mockStrategy.Setup(s => s.WrapStreamForDecompression(It.IsAny<Stream>()))
                        .Returns(expected);
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitLooseFilesDictionary db = new GitLooseFilesDictionary(fs, mockStrategy.Object);
            fs.WriteTestFile(@"te\st", "Foo");

            // Act
            using (Stream actual = db.OpenRead("test"))
            {
                Assert.Same(expected, actual);
            }
        }

        [Fact]
        public void OpenWriteUsesCompressionStrategyToWrapStreamForCompression()
        {
            // Arrange
            Mock<ICompressionStrategy> mockStrategy = new Mock<ICompressionStrategy>(MockBehavior.Strict);
            Stream expected = new MemoryStream();
            mockStrategy.Setup(s => s.WrapStreamForCompression(It.IsAny<Stream>()))
                        .Returns(expected);
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitLooseFilesDictionary db = new GitLooseFilesDictionary(fs, mockStrategy.Object);
            
            // Act
            using (Stream actual = db.OpenWrite("test", create: true))
            {
                Assert.Same(expected, actual);
            }
        }

        [Fact]
        public void OpenReadSupportsPartialKeyIfSingleMatchFound()
        {
            // Arrange
            InMemoryFileSystem fs = new InMemoryFileSystem();
            GitLooseFilesDictionary db = new GitLooseFilesDictionary(fs, new NullCompressionStrategy());
            fs.WriteTestFile(@"ab\cdefghijk", "Foo");

            // Act
            using (StreamReader reader = new StreamReader(db.OpenRead("abc")))
            {
                // Assert
                Assert.Equal("Foo", reader.ReadToEnd());
            }
        }
    }
}
