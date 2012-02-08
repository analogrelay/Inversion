using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Inversion.Data;
using Moq;
using Inversion.Storage;
using System.IO;
using Inversion.Utils;
using System.Security.Cryptography;

namespace Inversion.Core.Facts.Data
{
    public class DatabaseFacts
    {
        [Fact]
        public void ConstructorRequiresNonNullReferenceDirectory()
        {
            Assert.Throws<ArgumentNullException>(() => new Database(new HashGenerator(new SHA1Managed()), null, new Mock<IPersistentDictionary>().Object, new GitObjectCodec(), new Mock<IPackedObjectDatabase>().Object))
                  .WithParamName("directory");
        }

        [Fact]
        public void ConstructorRequiresNonNullDictionaryStorage()
        {
            Assert.Throws<ArgumentNullException>(() => new Database(new HashGenerator(new SHA1Managed()), new Mock<IReferenceDirectory>().Object, null, new GitObjectCodec(), new Mock<IPackedObjectDatabase>().Object))
                  .WithParamName("storage");
        }

        [Fact]
        public void ConstructorRequiresNonNullObjectDecoder()
        {
            Assert.Throws<ArgumentNullException>(() => new Database(new HashGenerator(new SHA1Managed()), new Mock<IReferenceDirectory>().Object, new Mock<IPersistentDictionary>().Object, null, new Mock<IPackedObjectDatabase>().Object))
                  .WithParamName("codec");
        }

        [Fact]
        public void GetObjectRequiresNonNullOrEmptyHash()
        {
            Database db = new Database(new HashGenerator(new SHA1Managed()), new Mock<IReferenceDirectory>().Object, new Mock<IPersistentDictionary>().Object, new GitObjectCodec(), new Mock<IPackedObjectDatabase>().Object);
            Assert.Throws<ArgumentException>(() => db.GetObject(null))
                  .WithParamName("hash")
                  .WithMessage(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "hash");
            Assert.Throws<ArgumentException>(() => db.GetObject(String.Empty))
                  .WithParamName("hash")
                  .WithMessage(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "hash");
        }

        [Fact]
        public void ResolveReferenceRequiresNonNullOrEmptyReferenceName()
        {
            Database db = new Database(new HashGenerator(new SHA1Managed()), new Mock<IReferenceDirectory>().Object, new Mock<IPersistentDictionary>().Object, new GitObjectCodec(), new Mock<IPackedObjectDatabase>().Object);
            Assert.Throws<ArgumentException>(() => db.ResolveReference(null))
                  .WithParamName("referenceName")
                  .WithMessage(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "referenceName");
            Assert.Throws<ArgumentException>(() => db.ResolveReference(String.Empty))
                  .WithParamName("referenceName")
                  .WithMessage(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "referenceName");
        }

        [Fact]
        public void StoreObjectRequiresNonNullObject()
        {
            Database db = new Database(new HashGenerator(new SHA1Managed()), new Mock<IReferenceDirectory>().Object, new Mock<IPersistentDictionary>().Object, new GitObjectCodec(), new Mock<IPackedObjectDatabase>().Object);
            Assert.Throws<ArgumentNullException>(() => db.StoreObject(null))
                  .WithParamName("obj");
        }

        [Fact]
        public void GetObjectReturnsNullIfObjectDoesNotExist()
        {
            // Arrange
            Mock<IPersistentDictionary> mockStorage = new Mock<IPersistentDictionary>();
            Database db = new Database(new HashGenerator(new SHA1Managed()), new Mock<IReferenceDirectory>().Object, mockStorage.Object, new GitObjectCodec(), new Mock<IPackedObjectDatabase>().Object);
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
            Database db = new Database(new HashGenerator(new SHA1Managed()), new Mock<IReferenceDirectory>().Object, mockStorage.Object, mockCodec.Object, new Mock<IPackedObjectDatabase>().Object);
            Stream expectedStrm = new MemoryStream();
            DatabaseObject expectedObj = DatabaseObject.Null();
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
            const string knownHash = "44ded774dda9dd8d53dcf37b7c77375180ab45dd";
            Mock<IPersistentDictionary> mockStorage = new Mock<IPersistentDictionary>(MockBehavior.Strict);
            Database db = new Database(new HashGenerator(new SHA1Managed()), new Mock<IReferenceDirectory>().Object, mockStorage.Object, new GitObjectCodec(), new Mock<IPackedObjectDatabase>().Object);
            using (MemoryStream expectedStrm = new MemoryStream())
            {
                DatabaseObject expectedObj = DatabaseObject.Null();
                mockStorage.Setup(s => s.OpenWrite(knownHash, /* create */ true)).Returns(expectedStrm);

                // Act
                db.StoreObject(expectedObj);
                expectedStrm.Flush();

                // Assert
                Assert.Equal("null 0\0", Encoding.ASCII.GetString(expectedStrm.ToArray()));
            }
        }

        [Fact]
        public void ComputHashReturnsHashWithoutAffectingDictionary()
        {
            // Arrange
            const string knownHash = "44ded774dda9dd8d53dcf37b7c77375180ab45dd";
            Mock<IPersistentDictionary> mockStorage = new Mock<IPersistentDictionary>(MockBehavior.Strict);
            Database db = new Database(new HashGenerator(new SHA1Managed()), new Mock<IReferenceDirectory>().Object, mockStorage.Object, new GitObjectCodec(), new Mock<IPackedObjectDatabase>().Object);
            using (MemoryStream expectedStrm = new MemoryStream())
            {
                DatabaseObject expectedObj = DatabaseObject.Null();
                
                // Act
                string hash = db.ComputeHash(expectedObj);

                // Assert
                Assert.Equal(knownHash, hash);
            }
        }

        [Fact]
        public void ResolveReferencePassesThroughReferenceDirectory()
        {
            // Arrange
            Mock<IReferenceDirectory> mockDirectory = new Mock<IReferenceDirectory>();
            Database db = new Database(new HashGenerator(new SHA1Managed()), mockDirectory.Object, new Mock<IPersistentDictionary>().Object, new GitObjectCodec(), new Mock<IPackedObjectDatabase>().Object);
            mockDirectory.Setup(r => r.ResolveReference("foo")).Returns("bar");

            // Act/Assert
            Assert.Equal("bar", db.ResolveReference("foo"));
        }
    }
}
