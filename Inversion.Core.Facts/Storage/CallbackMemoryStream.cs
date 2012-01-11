using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Inversion.Core.Facts.Storage
{
    public class CallbackMemoryStream : Stream
    {
        private bool _writeable;
        private MemoryStream _strm;
        private Action<byte[]> _callback;

        public override bool CanRead
        {
            get { return _strm.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _strm.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _writeable; }
        }

        public override long Length
        {
            get { return _strm.Length; }
        }

        public override long Position
        {
            get { return _strm.Position; }
            set { _strm.Position = value; }
        }

        public CallbackMemoryStream(MemoryStream strm, Action<byte[]> callback, bool writeable)
        {
            _strm = strm;
            _writeable = writeable;
            _callback = callback;
        }

        public override void Flush()
        {
            _strm.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _strm.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _strm.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if (!_writeable) { throw new NotSupportedException(); }
            _strm.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_writeable) { throw new NotSupportedException(); }
            _strm.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if(disposing) {
                _strm.Dispose();
                _callback(_strm.ToArray());
            }
        }
    }
}
