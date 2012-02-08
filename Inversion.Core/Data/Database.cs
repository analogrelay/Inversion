using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Storage;
using System.IO;
using System.Globalization;
using Inversion.Utils;

namespace Inversion.Data
{
    public class Database
    {
        internal HashGenerator HashGenerator { get; private set; }
        internal IReferenceDirectory Directory { get; private set; }
        internal IPersistentDictionary Storage { get; private set; }
        internal IPackedObjectDatabase PackedObjects { get; private set; }
        internal IObjectCodec Codec { get; private set; }

        // Inheritors can avoid setting Storage and Codec, at their own risk...
        [Obsolete("Be careful when using this constructor as the Storage and Codec properties will be null.")]
        protected Database() { }
        public Database(HashGenerator hashGenerator, IReferenceDirectory directory, IPersistentDictionary storage, IObjectCodec codec, IPackedObjectDatabase packedObjects)
        {
            if (hashGenerator == null) { throw new ArgumentNullException("hashAlgorithm"); }
            if (directory == null) { throw new ArgumentNullException("directory"); }
            if (storage == null) { throw new ArgumentNullException("storage"); }
            if (codec == null) { throw new ArgumentNullException("codec"); }
            if (packedObjects == null) { throw new ArgumentNullException("packedObjects"); }
            HashGenerator = hashGenerator;
            Directory = directory;
            Storage = storage;
            Codec = codec;
            PackedObjects = packedObjects;
        }

        public virtual DatabaseObject GetObject(string hash)
        {
            if (String.IsNullOrEmpty(hash)) { throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "hash"), "hash"); }

            if (Storage.Exists(hash))
            {
                return Codec.Decode(Storage.OpenRead(hash));
            }
            else
            {
                return PackedObjects.GetObject(hash);
            }
        }

        public virtual string ResolveReference(string referenceName)
        {
            if (String.IsNullOrEmpty(referenceName)) { throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "referenceName"), "referenceName"); }
            return Directory.ResolveReference(referenceName);
        }

        public virtual string StoreObject(DatabaseObject obj)
        {
            if (obj == null) { throw new ArgumentNullException("obj"); }

            var tup = ComputeHashCore(obj);
            string hash = tup.Item1;
            byte[] encoded = tup.Item2;

            using (Stream strm = Storage.OpenWrite(hash, create: true))
            {
                strm.Write(encoded, 0, encoded.Length);
            }
            return hash;
        }

        public string ComputeHash(DatabaseObject obj)
        {
            if (obj == null) { throw new ArgumentNullException("obj"); }
            return ComputeHashCore(obj).Item1;
        }

        private Tuple<string, byte[]> ComputeHashCore(DatabaseObject obj) {
            
            // Encode the data to a memory stream.
            // PERF: This is not a great plan methinks... If this becomes a memory sink, we should write it to a file maybe?
            //  Alternatively we could stream the data out to a temp file while ALSO building the hash, then move the temp file to the right place
            //  based on the hash
            byte[] encoded;
            using (MemoryStream encoderOutput = new MemoryStream())
            {
                Codec.Encode(obj, encoderOutput);
                encoderOutput.Flush();
                encoded = encoderOutput.ToArray();
            }

            // Hash the data
            string hash = HashGenerator.HashData(encoded);
            return Tuple.Create(hash, encoded);
        }
    }
}
