using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Inversion.Data;
using System.Security.Cryptography;
using Moq;
using System.IO;

namespace Inversion.Core.Facts.Data
{
    public class HashGeneratorFacts
    {
        [Fact]
        public void ConstructorRequiresNonNullHashAlgorithm()
        {
            Assert.Throws<ArgumentNullException>(() => new HashGenerator(null))
                  .WithParamName("algorithm");
        }

        [Fact]
        public void HashDataRequiresNonNullByteArrayData()
        {
            Assert.Throws<ArgumentNullException>(() => new HashGenerator(new SHA1Managed()).HashData((byte[])null))
                  .WithParamName("data");
        }

        [Fact]
        public void HashDataRequiresNonNullStreamData()
        {
            Assert.Throws<ArgumentNullException>(() => new HashGenerator(new SHA1Managed()).HashData((Stream)null))
                  .WithParamName("data");
        }

        [Fact]
        public void HashGeneratorUsesAlgorithmToComputeHashGivenByteArray()
        {
            // Arrange
            const string input = "Foo Bar Baz";
            const string expected = "759aef3d77fbc8e6114ea17e9283adff3d987195";
            HashGenerator gen = new HashGenerator(new SHA1Managed());
            
            // Act
            string actual = gen.HashData(Encoding.ASCII.GetBytes(input));

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void HashGeneratorUsesAlgorithmToComputeHashGivenStream()
        {
            // Arrange
            const string input = "Foo Bar Baz";
            const string expected = "759aef3d77fbc8e6114ea17e9283adff3d987195";
            HashGenerator gen = new HashGenerator(new SHA1Managed());

            // Act
            string actual;
            using (Stream strm = new MemoryStream(Encoding.ASCII.GetBytes(input)))
            {
                actual = gen.HashData(strm);
            }

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
