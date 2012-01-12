using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Inversion.Data;
using Moq;
using Inversion.Storage;
using System.IO;

namespace Inversion.Core.Facts.Data
{
    public class DatabaseFacts
    {
        [Fact]
        public void ConstructorRequiresNonNullDictionaryStorage()
        {
            Assert.Equal("storage",
                Assert.Throws<ArgumentNullException>(() => new Database(null, new GitObjectCodec())).ParamName);
        }

        [Fact]
        public void ConstructorRequiresNonNullObjectDecoder()
        {
            Assert.Equal("codec",
                Assert.Throws<ArgumentNullException>(() => new Database(new Mock<IPersistentDictionary>().Object, null)).ParamName);
        }

        [Fact]
        public void GetObjectRequiresNonNullOrEmptyHash()
        {
            Assert.Equal("hash",
                Assert.Throws<ArgumentException>(
                    () => new Database(new Mock<IPersistentDictionary>().Object, new GitObjectCodec())
                        .GetObject(null)).ParamName);
            Assert.Equal("hash",
                Assert.Throws<ArgumentException>(
                    () => new Database(new Mock<IPersistentDictionary>().Object, new GitObjectCodec())
                        .GetObject(String.Empty)).ParamName);
        }

        [Fact]
        public void StoreObjectRequiresNonNullOrEmptyHash()
        {
            Assert.Equal("hash",
                Assert.Throws<ArgumentException>(
                    () => new Database(new Mock<IPersistentDictionary>().Object, new GitObjectCodec())
                        .StoreObject(null, new NullDatabaseObject())).ParamName);
            Assert.Equal("hash",
                Assert.Throws<ArgumentException>(
                    () => new Database(new Mock<IPersistentDictionary>().Object, new GitObjectCodec())
                        .StoreObject(String.Empty, new NullDatabaseObject())).ParamName);
        }

        [Fact]
        public void StoreObjectRequiresNonNullObject()
        {
            Assert.Equal("obj",
                Assert.Throws<ArgumentNullException>(
                    () => new Database(new Mock<IPersistentDictionary>().Object, new GitObjectCodec())
                        .StoreObject("abc", null)).ParamName);
        }

        [Fact]
        public void GetObjectReturnsNullIfObjectDoesNotExist()
        {
            // Arrange
            Mock<IPersistentDictionary> mockStorage = new Mock<IPersistentDictionary>();
            Database db = new Database(mockStorage.Object, new GitObjectCodec());
            mockStorage.Setup(s => s.Exists("abcdefghij")).Returns(false);

            // Act/Assert
            Assert.Null(db.GetObject("abcdefghij"));
        }

        [Fact]
        public void GetObjectReturnsDecodedObjectIfObjectExists()
        {
            // Arrange
            Mock<IPersistentDictionary> mockStorage = new Mock<IPersistentDictionary>(MockBehavior.Strict);
            Mock<IObjectCodec> mockCodec = new Mock<IObjectCodec>(MockBehavior.Strict);
            Database db = new Database(mockStorage.Object, mockCodec.Object);
            Stream expectedStrm = new MemoryStream();
            DatabaseObject expectedObj = new NullDatabaseObject();
            mockStorage.Setup(s => s.Exists("abcdefghij")).Returns(true);
            mockStorage.Setup(s => s.OpenRead("abcdefghij")).Returns(expectedStrm);
            mockCodec.Setup(d => d.Decode(expectedStrm)).Returns(expectedObj);

            // Act/Assert
            Assert.Same(expectedObj, db.GetObject("abcdefghij"));
        }

        [Fact]
        public void StoreObjectStoresEncodedObject()
        {
            // Arrange
            Mock<IPersistentDictionary> mockStorage = new Mock<IPersistentDictionary>(MockBehavior.Strict);
            Mock<IObjectCodec> mockCodec = new Mock<IObjectCodec>(MockBehavior.Loose);
            Database db = new Database(mockStorage.Object, mockCodec.Object);
            Stream expectedStrm = new MemoryStream();
            DatabaseObject expectedObj = new NullDatabaseObject();
            mockStorage.Setup(s => s.OpenWrite("abcdefghij", /* create */ true)).Returns(expectedStrm);
            
            // Act
            db.StoreObject("abcdefghij", expectedObj);

            // Assert
            mockCodec.Verify(d => d.Encode(expectedObj, expectedStrm));
        }
    }
}
