using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Inversion.Utils;
using System.Globalization;

namespace Inversion.Data
{
    class WindowedStream : Stream
    {
        private long _base;
        private Stream _inner;

        public WindowedStream(Stream inner) {
            if (inner == null) { throw new ArgumentNullException("inner"); }
            _inner = inner;
            _base = inner.Position;
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
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            if (offset < 0) { throw new ArgumentOutOfRangeException("offset", String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Negative, "offset")); }
            if (count < 0) { throw new ArgumentOutOfRangeException("count", String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Negative, "count")); }

            EnsurePosition();
            return _inner.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin < SeekOrigin.Begin || origin > SeekOrigin.End) { throw new ArgumentOutOfRangeException("origin", String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Must_Be_Valid_Enum_Value, "origin", typeof(SeekOrigin).FullName)); }
            if (origin == SeekOrigin.Begin && offset < 0) { throw new ArgumentOutOfRangeException("offset", String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Negative, "offset")); }
            
            EnsurePosition();

            long newLocation;
            switch (origin)
            {
                case SeekOrigin.Current:
                    newLocation = _inner.Position + offset;
                    break;
                case SeekOrigin.End:
                    newLocation = _inner.Length + offset;
                    break;
                default:
                    newLocation = offset + _base;
                    offset += _base;
                    break;
            }
            if(newLocation < _base) {
                throw new IOException("An attempt was made to seek the stream to a location before the beginning.");
            }

            return _inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if (value < 0) { throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Negative, "value")); }
            _inner.SetLength(value + _base);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            if (offset < 0) { throw new ArgumentOutOfRangeException("offset", String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Negative, "offset")); }
            if (count < 0) { throw new ArgumentOutOfRangeException("count", String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Negative, "count")); }

            EnsurePosition();
            _inner.Write(buffer, offset, count);
        }

        private void EnsurePosition()
        {
            if (_inner.Position < _base)
            {
                throw new InvalidOperationException("Somehow the WindowedStream's inner stream ended up pointing before the base, once you've wrapped a Stream in a ConstrainedStream, do not adjust the position of the inner stream");
            }
        }
    }
}
