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
            Assert.Equal("source",
                Assert.Throws<ArgumentNullException>(() => new GitObjectCodec().Decode(null)).ParamName);
        }

        [Fact]
        public void EncodeRequiresNonNullObject()
        {
            Assert.Equal("obj",
                Assert.Throws<ArgumentNullException>(() => new GitObjectCodec().Encode(null, Stream.Null)).ParamName);
        }

        [Fact]
        public void EncodeRequiresNonNullTarget()
        {
            Assert.Equal("target",
                Assert.Throws<ArgumentNullException>(() => new GitObjectCodec().Encode(new NullDatabaseObject(), null)).ParamName);
        }
    }
}
