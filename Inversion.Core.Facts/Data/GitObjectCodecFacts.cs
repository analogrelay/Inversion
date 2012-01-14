using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Inversion.Data;
using System.IO;

namespace Inversion.Core.Facts.Data
{
    public class GitObjectCodecFacts
    {
        [Fact]
        public void DecodeRequiresNonNullSource()
        {
            Assert.Throws<ArgumentNullException>(() => new GitObjectCodec().Decode(null))
                  .WithParamName("source");
        }

        [Fact]
        public void EncodeRequiresNonNullObject()
        {
            Assert.Throws<ArgumentNullException>(() => new GitObjectCodec().Encode(null, Stream.Null))
                  .WithParamName("obj");
        }

        [Fact]
        public void EncodeRequiresNonNullTarget()
        {
            Assert.Throws<ArgumentNullException>(() => new GitObjectCodec().Encode(new NullDatabaseObject(), null))
                  .WithParamName("target");
        }
    }
}
