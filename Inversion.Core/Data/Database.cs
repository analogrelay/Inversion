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
        internal IReferenceDirectory Directory { get; private set; }
        internal IPersistentDictionary Storage { get; private set; }
        internal IObjectCodec Codec { get; private set; }

        // Inheritors can avoid setting Storage and Codec, at their own risk...
        [Obsolete("Be careful when using this constructor as the Storage and Codec properties will be null.")]
        protected Database() { }
        public Database(IReferenceDirectory directory, IPersistentDictionary storage, IObjectCodec codec) {
            if (directory == null) { throw new ArgumentNullException("directory"); }
            if (storage == null) { throw new ArgumentNullException("storage"); }
            if (codec == null) { throw new ArgumentNullException("codec"); }
            Directory = directory;
            Storage = storage;
            Codec = codec;
        }

        public virtual DatabaseObject GetObject(string hash)
        {
            if (String.IsNullOrEmpty(hash)) { throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "hash"), "hash"); }

            if (Storage.Exists(hash))
            {
                return Codec.Decode(Storage.OpenRead(hash));
            }
            return null;
        }

        public virtual string ResolveReference(string referenceName)
        {
            if (String.IsNullOrEmpty(referenceName)) { throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "referenceName"), "referenceName"); }
            return Directory.ResolveReference(referenceName);
        }

        public virtual void StoreObject(string hash, DatabaseObject obj)
        {
            if (String.IsNullOrEmpty(hash)) { throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "hash"), "hash"); }
            if (obj == null) { throw new ArgumentNullException("obj"); }

            using (Stream strm = Storage.OpenWrite(hash, create: true))
            {
                Codec.Encode(obj, strm);
            }
        }
    }
}
