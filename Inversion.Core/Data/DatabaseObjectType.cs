using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inversion.Data
{
    public enum DatabaseObjectType : byte
    {
        Commit = 0,
        Tree = 1,
        Blob = 2,
        Tag = 3,
        OffsetDelta = 6,
        HashDelta = 7,
        Null = 255
    }

    internal static class DatabaseObjectTypeHelper
    {
        private static readonly Dictionary<string, DatabaseObjectType> _mappings = new Dictionary<string, DatabaseObjectType>(StringComparer.OrdinalIgnoreCase) {
            { "commit", DatabaseObjectType.Commit },
            { "tree", DatabaseObjectType.Tree },
            { "blob", DatabaseObjectType.Blob },
            { "tag", DatabaseObjectType.Tag }
        };

        public static string ToStandardString(DatabaseObjectType type)
        {
            switch (type)
            {
                case DatabaseObjectType.Commit: return "commit";
                case DatabaseObjectType.Tree: return "tree";
                case DatabaseObjectType.Blob: return "blob";
                case DatabaseObjectType.Tag: return "tag";
                case DatabaseObjectType.OffsetDelta: return "<<offset-delta>>";
                case DatabaseObjectType.HashDelta: return "<<hash-delta>>";
                case DatabaseObjectType.Null: return "<<null>>";
                default: return "<<unknown>>";
            }
        }

        public static DatabaseObjectType Parse(string str)
        {
            DatabaseObjectType type;
            if (_mappings.TryGetValue(str, out type))
            {
                return type;
            }
            return DatabaseObjectType.Null;
        }
    }
}
