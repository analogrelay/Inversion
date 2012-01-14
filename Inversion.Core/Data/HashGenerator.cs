using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Inversion.Data
{
    public class HashGenerator
    {
        private HashAlgorithm _algorithm;

        public HashGenerator(HashAlgorithm algorithm)
        {
            if (algorithm == null) { throw new ArgumentNullException("algorithm"); }
            _algorithm = algorithm;
        }

        public virtual string HashData(byte[] data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            return HashToString(_algorithm.ComputeHash(data));
        }

        public virtual string HashData(Stream data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            return HashToString(_algorithm.ComputeHash(data));
        }

        private static string HashToString(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", String.Empty).ToLowerInvariant();
        }
    }
}
