using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Inversion.Storage;
using System.IO;
using Ionic.Zlib;

namespace Inversion.Core.Facts.Storage
{
    public class ZlibCompressionStrategyFacts
    {
        [Fact]
        public void WrapForDecompressionRequiresNonNullStream()
        {
            Assert.Throws<ArgumentNullException>(() => new ZlibCompressionStrategy().WrapStreamForDecompression(null))
                  .WithParamName("target");
        }

        [Fact]
        public void WrapForCompressionRequiresNonNullStream()
        {
            Assert.Throws<ArgumentNullException>(() => new ZlibCompressionStrategy().WrapStreamForCompression(null))
                  .WithParamName("target");
        }

        [Fact]
        public void WhatIsCompressedCanBeDecompressed()
        {
            const string expected = "FooBarBaz";
            ZlibCompressionStrategy strategy = new ZlibCompressionStrategy();

            byte[] compressed;
            using (MemoryStream target = new MemoryStream())
            {
                using (StreamWriter compresser = new StreamWriter(strategy.WrapStreamForCompression(target)))
                {
                    compresser.Write(expected);
                }
                target.Flush();
                compressed = target.ToArray();
            }

            using (MemoryStream source = new MemoryStream(compressed))
            {
                using (StreamReader decompresser = new StreamReader(strategy.WrapStreamForDecompression(source)))
                {
                    Assert.Equal(expected, decompresser.ReadToEnd());
                }
            }
        }
    }
}
