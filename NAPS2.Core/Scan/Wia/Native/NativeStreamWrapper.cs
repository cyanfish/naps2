using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace NAPS2.Scan.Wia.Native
{
    public class NativeStreamWrapper : Stream
    {
        private readonly IStream source;
        private readonly IntPtr nativeLong;

        private long position;

        public NativeStreamWrapper(IStream source)
        {
            this.source = source;
            nativeLong = Marshal.AllocCoTaskMem(8);
        }

        ~NativeStreamWrapper()
        {
            Marshal.FreeCoTaskMem(nativeLong);
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override void Flush()
        {
            source.Commit(0);
        }

        public override long Length
        {
            get
            {
                source.Stat(out var stat, 1);
                return stat.cbSize;
            }
        }

        public override long Position
        {
            get => position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset != 0) throw new NotImplementedException();
            source.Read(buffer, count, nativeLong);
            int bytesRead = Marshal.ReadInt32(nativeLong);
            position += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                position = offset;
            }
            else if (origin == SeekOrigin.Current)
            {
                position += offset;
            }
            else
            {
                throw new NotImplementedException();
            }
            source.Seek(offset, (int)origin, nativeLong);
            return Marshal.ReadInt64(nativeLong);
        }

        public override void SetLength(long value)
        {
            source.SetSize(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset != 0) throw new NotImplementedException();
            source.Write(buffer, count, IntPtr.Zero);
            position += count;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Marshal.Release(Marshal.GetIUnknownForObject(source));
        }
    }
}
