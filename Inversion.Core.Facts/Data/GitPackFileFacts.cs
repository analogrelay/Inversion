using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
