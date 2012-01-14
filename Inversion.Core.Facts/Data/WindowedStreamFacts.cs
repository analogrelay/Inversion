using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Inversion.Data;
using System.IO;
using Moq;
using System.Linq.Expressions;
using Xunit.Extensions;
using Inversion.Utils;

namespace Inversion.Core.Facts.Data
{
    public class WindowedStreamFacts
    {
        [Fact]
        public void ConstructorRequiresNonNullStream()
        {
            Assert.Throws<ArgumentNullException>(() => new WindowedStream(null))
                  .WithParamName("inner");
        }

        [Fact]
        public void ReadRequiresNonNullBuffer()
        {
            Assert.Throws<ArgumentNullException>(() => new WindowedStream(new Mock<Stream>().Object).Read(null, 0, 0))
                  .WithParamName("buffer");
        }

        [Fact]
        public void ReadRequiresNonNegativeOffset()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WindowedStream(new Mock<Stream>().Object).Read(new byte[0], -1, 0))
                  .WithParamName("offset")
                  .WithMessage(String.Format(CommonResources.Argument_Cannot_Be_Negative, "offset"));
        }

        [Fact]
        public void ReadRequiresNonNegativeCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WindowedStream(new Mock<Stream>().Object).Read(new byte[0], 0, -1))
                  .WithParamName("count")
                  .WithMessage(String.Format(CommonResources.Argument_Cannot_Be_Negative, "count"));
        }

        [Fact]
        public void WriteRequiresNonNullBuffer()
        {
            Assert.Throws<ArgumentNullException>(() => new WindowedStream(new Mock<Stream>().Object).Write(null, 0, 0))
                  .WithParamName("buffer");
        }

        [Fact]
        public void WriteRequiresNonNegativeOffset()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WindowedStream(new Mock<Stream>().Object).Write(new byte[0], -1, 0))
                  .WithParamName("offset")
                  .WithMessage(String.Format(CommonResources.Argument_Cannot_Be_Negative, "offset"));
        }

        [Fact]
        public void WriteRequiresNonNegativeCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WindowedStream(new Mock<Stream>().Object).Write(new byte[0], 0, -1))
                  .WithParamName("count")
                  .WithMessage(String.Format(CommonResources.Argument_Cannot_Be_Negative, "count"));
        }

        [Fact]
        public void SetLengthRequiresNonNegativeLength()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WindowedStream(new Mock<Stream>().Object).SetLength(-1))
                  .WithParamName("value")
                  .WithMessage(String.Format(CommonResources.Argument_Cannot_Be_Negative, "value"));
        }

        [Fact]
        public void SeekRequiresNonNegativeOffsetForBeginOrigin()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WindowedStream(new Mock<Stream>().Object).Seek(-1, SeekOrigin.Begin))
                  .WithParamName("offset")
                  .WithMessage(String.Format(CommonResources.Argument_Cannot_Be_Negative, "offset"));
        }

        [Fact]
        public void SeekRequiresValidOrigin()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WindowedStream(new Mock<Stream>().Object).Seek(0, (SeekOrigin)(-1)))
                  .WithParamName("origin")
                  .WithMessage(String.Format(CommonResources.Argument_Must_Be_Valid_Enum_Value, "origin", typeof(SeekOrigin).FullName));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WindowedStream(new Mock<Stream>().Object).Seek(0, (SeekOrigin)3))
                  .WithParamName("origin")
                  .WithMessage(String.Format(CommonResources.Argument_Must_Be_Valid_Enum_Value, "origin", typeof(SeekOrigin).FullName));
        }

        [Fact]
        public void CanReadIsPassedThrough()
        {
            RunPassThroughTest(s => s.CanRead, true);
        }

        [Fact]
        public void CanWriteIsPassedThrough()
        {
            RunPassThroughTest(s => s.CanWrite, true);
        }

        [Fact]
        public void CanSeekIsPassedThrough()
        {
            RunPassThroughTest(s => s.CanSeek, true);
        }

        [Fact]
        public void FlushIsPassedThrough()
        {
            RunPassThroughTest(s => s.Flush());
        }

        [Fact]
        public void SetLengthIsPassedThrough()
        {
            RunPassThroughTest(s => s.SetLength(12));
        }

        [Fact]
        public void LengthReturnsActualLengthMinusBase()
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.Setup(s => s.Position).Returns(42);
            mockStream.Setup(s => s.Length).Returns(84);
            WindowedStream strm = new WindowedStream(mockStream.Object);

            // Act/Assert
            Assert.Equal(42, strm.Length);
        }

        [Fact]
        public void PositionReturnsActualPositionMinusBase()
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.Setup(s => s.Position).Returns(42);
            WindowedStream strm = new WindowedStream(mockStream.Object);
            
            // Act (move the underlying stream)
            mockStream.Setup(s => s.Position).Returns(52);

            // Assert
            Assert.Equal(10, strm.Position);

        }

        [Fact]
        public void SettingPositionAddsBaseBeforeCallingInnerStream()
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.SetupProperty(s => s.Position, 42);
            WindowedStream strm = new WindowedStream(mockStream.Object);

            // Act (move the underlying stream)
            strm.Position = 10;

            // Assert
            Assert.Equal(52, mockStream.Object.Position);
        }

        [Fact]
        public void SeekFromCurrentJustAdvancesByThatAmount()
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.SetupProperty(s => s.Position, 42);
            WindowedStream strm = new WindowedStream(mockStream.Object);

            // Act
            strm.Seek(10, SeekOrigin.Current);

            // Assert
            mockStream.Verify(s => s.Seek(10, SeekOrigin.Current));
        }

        [Fact]
        public void SeekFromEndMovesBackThatAmount()
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.SetupProperty(s => s.Position, 42);
            mockStream.Setup(s => s.Length).Returns(84);
            WindowedStream strm = new WindowedStream(mockStream.Object);

            // Act
            strm.Seek(10, SeekOrigin.End);

            // Assert
            mockStream.Verify(s => s.Seek(10, SeekOrigin.End));
        }

        [Fact]
        public void SeekFromEndThrowsIOExceptionIfMovementWouldTakeItBeforeBase()
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.SetupProperty(s => s.Position, 42);
            mockStream.Setup(s => s.Length).Returns(84);
            WindowedStream strm = new WindowedStream(mockStream.Object);

            // Act/Assert
            Assert.Throws<IOException>(() => strm.Seek(-50, SeekOrigin.End))
                  .WithMessage("An attempt was made to seek the stream to a location before the beginning.");
        }

        [Fact]
        public void SeekThrowsIfNewLocationIsBeforeBase()
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.SetupProperty(s => s.Position, 42);
            WindowedStream strm = new WindowedStream(mockStream.Object);

            // Act/Assert
            Assert.Throws<IOException>(() => strm.Seek(-10, SeekOrigin.Current))
                  .WithMessage("An attempt was made to seek the stream to a location before the beginning.");
        }

        [Fact]
        public void ReadPassesThroughIfWithinWindow()
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.SetupProperty(s => s.Position, 42);
            WindowedStream strm = new WindowedStream(mockStream.Object);
            mockStream.Setup(s => s.Read(new byte[0], 0, 0)).Returns(12);

            // Act
            int actual = strm.Read(new byte[0], 0, 0);

            // Assert
            Assert.Equal(12, actual);
        }

        [Fact]
        public void ReadThrowsInvalidOperationExceptionIfOutsideWindow()
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.SetupProperty(s => s.Position, 42);
            WindowedStream strm = new WindowedStream(mockStream.Object);
            mockStream.Object.Position = 21;

            // Act/Assert
            Assert.Throws<InvalidOperationException>(() => strm.Read(new byte[0], 0, 0))
                  .WithMessage("Somehow the ConstrainedStream's inner stream ended up pointing before the base, once you've wrapped a Stream in a ConstrainedStream, do not adjust the position of the inner stream");
        }

        [Fact]
        public void SeekPassesThroughIfWithinWindow()
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.SetupProperty(s => s.Position, 42);
            WindowedStream strm = new WindowedStream(mockStream.Object);
            mockStream.Setup(s => s.Seek(52, SeekOrigin.Begin)).Returns(12L);

            // Act
            long actual = strm.Seek(10, SeekOrigin.Begin);

            // Assert
            Assert.Equal(12L, actual);
        }

        [Fact]
        public void SeekThrowsInvalidOperationExceptionIfOutsideWindow()
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.SetupProperty(s => s.Position, 42);
            WindowedStream strm = new WindowedStream(mockStream.Object);
            mockStream.Object.Position = 21;

            // Act/Assert
            Assert.Throws<InvalidOperationException>(() => strm.Seek(0, SeekOrigin.Begin))
                  .WithMessage("Somehow the ConstrainedStream's inner stream ended up pointing before the base, once you've wrapped a Stream in a ConstrainedStream, do not adjust the position of the inner stream");
        }

        [Fact]
        public void WritePassesThroughIfWithinWindow()
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.SetupProperty(s => s.Position, 42);
            WindowedStream strm = new WindowedStream(mockStream.Object);
            
            // Act
            strm.Write(new byte[0], 0, 0);

            // Assert
            mockStream.Verify(s => s.Write(new byte[0], 0, 0));
        }

        [Fact]
        public void WriteThrowsInvalidOperationExceptionIfOutsideWindow()
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.SetupProperty(s => s.Position, 42);
            WindowedStream strm = new WindowedStream(mockStream.Object);
            mockStream.Object.Position = 21;

            // Act/Assert
            Assert.Throws<InvalidOperationException>(() => strm.Write(new byte[0], 0, 0))
                  .WithMessage("Somehow the ConstrainedStream's inner stream ended up pointing before the base, once you've wrapped a Stream in a ConstrainedStream, do not adjust the position of the inner stream");
        }

        private void RunPassThroughTest<T>(Expression<Func<Stream, T>> act, T expected) {
            T actual = default(T);
            RunPassThroughTest<Func<Stream, T>>(
                act, 
                m => m.Setup(act).Returns(expected), 
                s => actual = act.Compile()(s), 
                _ => Assert.Equal(expected, actual));
        }

        private void RunPassThroughTest(Expression<Action<Stream>> act)
        {
            RunPassThroughTest<Action<Stream>>(
                act,
                m => {},
                act.Compile(), 
                m => m.Verify(act));
        }

        private void RunPassThroughTest<T>(Expression<T> act, Action<Mock<Stream>> setup, Action<Stream> extracter,  Action<Mock<Stream>> asserter)
        {
            // Arrange
            Mock<Stream> mockStream = new Mock<Stream>();
            WindowedStream target = new WindowedStream(mockStream.Object);
            setup(mockStream);

            // Act
            extracter(target);

            // Assert
            asserter(mockStream);
        }
    }
}
