using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Inversion.Data
{
    public class GitObjectCodec : IObjectCodec
    {
        public DatabaseObject Decode(Stream source)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            
            // Create a binary reader that can be disposed without disposing the underlying stream.
            string type;
            string lenStr;
            using (BinaryReader reader = new BinaryReader(new DisposeProtectedStream(source)))
            {
                // First token is the type
                type = ReadToken(reader);

                // Second is the len
                lenStr = ReadToken(reader);
            }

            int len = -1;
            if(!Int32.TryParse(lenStr, out len)) {
                throw new InvalidDataException(String.Format("Invalid object, length token value is not an integer: {0}", lenStr));
            }

            // Should now be at the object body, read that in (for now)
            return new DatabaseObject(type, len, new StreamObjectContent(new ConstrainedStream(source)));
        }

        public void Encode(DatabaseObject obj, Stream target)
        {
            if (obj == null) { throw new ArgumentNullException("obj"); }
            if (target == null) { throw new ArgumentNullException("target"); }
            
            // Write header
            using (BinaryWriter writer = new BinaryWriter(new DisposeProtectedStream(target)))
            {
                writer.Write(obj.Type.ToCharArray());
                writer.Write(' ');
                writer.Write(obj.Length.ToString().ToCharArray());
                writer.Write((byte)0);
            }

            // Write content
            obj.Content.WriteTo(target);
        }

        private string ReadToken(BinaryReader reader)
        {
            StringBuilder builder = new StringBuilder();
            char chr;
            while ((chr = reader.ReadChar()) != ' ' && chr != '\0')
            {
                builder.Append(chr);
            }
            return builder.ToString();
        }
    }
}
