using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Inversion.Core.Facts.Storage;
using Inversion.Data;
using Inversion.Delta;
using Inversion.Storage;
using Inversion.Utils;
using Moq;
using Xunit;

namespace Inversion.Core.Facts.Data
{
    public class GitPackFileFacts
    {
        public class Constructor
        {
            [Fact]
            public void RequiresNonNullFileSystem()
            {
                Assert.Throws<ArgumentNullException>(() => new GitPackFile(
                    fileSystem: null,
                    filename: "foo", 
                    compression: new ZlibCompressionStrategy(), 
                    index: new Mock<GitPackIndex>().Object, 
                    delta: new GitDeltaDecoder()))
                      .WithParamName("fileSystem");
            }

            [Fact]
            public void RequiresNonNullOrEmptyFileName()
            {
                Assert.Throws<ArgumentException>(() => new GitPackFile(
                    fileSystem: new InMemoryFileSystem(), 
                    filename: null, 
                    compression: new ZlibCompressionStrategy(), 
                    index: new Mock<GitPackIndex>().Object, 
                    delta: new GitDeltaDecoder()))
                      .WithMessage(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "filename")
                      .WithParamName("filename");
                Assert.Throws<ArgumentException>(() => new GitPackFile(
                    fileSystem: new InMemoryFileSystem(),
                    filename: String.Empty,
                    compression: new ZlibCompressionStrategy(),
                    index: new Mock<GitPackIndex>().Object,
                    delta: new GitDeltaDecoder()))
                      .WithMessage(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "filename")
                      .WithParamName("filename");
            }

            [Fact]
            public void RequiresNonNullCompression()
            {
                Assert.Throws<ArgumentNullException>(() => new GitPackFile(
                    fileSystem: new InMemoryFileSystem(),
                    filename: "foo",
                    compression: null,
                    index: new Mock<GitPackIndex>().Object,
                    delta: new GitDeltaDecoder()))
                      .WithParamName("compression");
            }

            [Fact]
            public void RequiresNonNullIndex()
            {
                Assert.Throws<ArgumentNullException>(() => new GitPackFile(
                    fileSystem: new InMemoryFileSystem(),
                    filename: "foo",
                    compression: new ZlibCompressionStrategy(),
                    index: null,
                    delta: new GitDeltaDecoder()))
                      .WithParamName("index");
            }

            [Fact]
            public void RequiresNonNullDelta()
            {
                Assert.Throws<ArgumentNullException>(() => new GitPackFile(
                    fileSystem: new InMemoryFileSystem(),
                    filename: "foo",
                    compression: new ZlibCompressionStrategy(),
                    index: new Mock<GitPackIndex>().Object,
                    delta: null))
                      .WithParamName("delta");
            }
        }

        public class Open
        {
            [Fact]
            public void RequiresNonNullFileSystem()
            {
                Assert.Throws<ArgumentNullException>(() => GitPackFile.Open(fs: null, baseName: "foo"))
                      .WithParamName("fs");
            }

            [Fact]
            public void RequiresNonNullorEmptyBaseName()
            {
                Assert.Throws<ArgumentException>(() => GitPackFile.Open(fs: new InMemoryFileSystem(), baseName: null))
                      .WithMessage(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "baseName")
                      .WithParamName("baseName");
                Assert.Throws<ArgumentException>(() => GitPackFile.Open(fs: new InMemoryFileSystem(), baseName: String.Empty))
                      .WithMessage(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "baseName")
                      .WithParamName("baseName");
            }

            [Fact]
            public void SetsUpDefaultCompressionAndDelta()
            {
                // Arrange
                InMemoryFileSystem fs = new InMemoryFileSystem();
                fs.WriteTestFile("pack-test.idx", w =>
                {
                    w.Write(GitPackIndex.V2PlusSignature);
                    w.Write(IPAddress.HostToNetworkOrder(2));
                });

                // Act
                GitPackFile file = GitPackFile.Open(fs, "pack-test");

                // Assert
                Assert.IsType<ZlibCompressionStrategy>(file.Compression);
                Assert.IsType<GitDeltaDecoder>(file.Delta);
                Assert.IsType<GitPackIndexV2>(file.Index);
            }

            [Fact]
            public void UsesCorrectPackFile()
            {
                // Arrange
                InMemoryFileSystem fs = new InMemoryFileSystem();
                fs.WriteTestFile("pack-test.idx", w =>
                {
                    w.Write(GitPackIndex.V2PlusSignature);
                    w.Write(IPAddress.HostToNetworkOrder(2));
                });

                // Act
                GitPackFile file = GitPackFile.Open(fs, "pack-test");

                // Assert
                Assert.Equal("pack-test.pack", file.PackFileName);
                Assert.Same(fs, file.FileSystem);
            }
        }

        public class Exists
        {
            [Fact]
            public void RequiresNonNullOrEmptyHash()
            {
                // Arrange
                TestableGitPackFile file = CreateTestPackFile();

                // Act/Assert
                Assert.Throws<ArgumentException>(() => file.Exists(hash: null))
                      .WithMessage(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "hash")
                      .WithParamName("hash");
                Assert.Throws<ArgumentException>(() => file.Exists(hash: String.Empty))
                      .WithMessage(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "hash")
                      .WithParamName("hash");
            }

            [Fact]
            public void ReturnsTrueIfHashExistsInIndex()
            {
                // Arrange
                byte[] hash = { 0x01, 0x23, 0x45 };
                TestableGitPackFile file = CreateTestPackFile();
                file.MockIndex
                    .Setup(i => i.EntryExists(hash))
                    .Returns(true);

                // Act/Assert
                Assert.True(file.Exists("012345"));
            }

            [Fact]
            public void ReturnsTrueIfHashDoesNotExistInIndex()
            {
                // Arrange
                TestableGitPackFile file = CreateTestPackFile();
                
                // Act/Assert
                Assert.False(file.Exists("012345"));
            }
        }

        private static TestableGitPackFile CreateTestPackFile()
        {
            return new TestableGitPackFile(
                new InMemoryFileSystem(),
                "pack-test.pack",
                new NullCompressionStrategy(),
                new Mock<GitPackIndex>(),
                new GitDeltaDecoder());
        }

        private class TestableGitPackFile : GitPackFile
        {
            public Mock<GitPackIndex> MockIndex { get; private set; }

            public TestableGitPackFile(
                IFileSystem fileSystem,
                string fileName,
                ICompressionStrategy compression,
                Mock<GitPackIndex> index,
                IDeltaDecoder delta)
                : base(fileSystem, fileName, compression, index.Object, delta)
            {
                MockIndex = index;
            }
        }
    }
}
