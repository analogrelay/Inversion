using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Inversion.Data
{
    class ConstrainedStream : Stream
    {
        private long _base;
        private Stream _inner;

        public ConstrainedStream(Stream inner) : this(inner, inner.Position) { }
        public ConstrainedStream(Stream inner, long @base)
        {
            _inner = inner;
            _base = @base;
        }

        public override bool CanRead
        {
            get { return _inner.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _inner.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _inner.CanWrite; }
        }

        public override void Flush()
        {
            _inner.Flush();
        }

        public override long Length
        {
            get { return _inner.Length - _base; }
        }

        public override long Position
        {
            get
            {
                return _inner.Position - _base;
            }
            set
            {
                _inner.Position = value + _base;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsurePosition();
            return _inner.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsurePosition();
            return _inner.Seek(offset + _base, origin);
        }

        public override void SetLength(long value)
        {
            _inner.SetLength(value + _base);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsurePosition();
            _inner.Write(buffer, offset, count);
        }

        private void EnsurePosition()
        {
            Debug.Assert(_inner.Position >= _base);
            if (_inner.Position < _base)
            {
                throw new InvalidOperationException("Somehow the ConstrainedStream's inner stream ended up pointing before the base, once you've wrapped a Stream in a ConstrainedStream, do not adjust the position of the inner stream");
            }
        }
    }
}
