using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaTransfer : NativeWiaObject
    {
        protected internal WiaTransfer(IntPtr handle) : base(handle)
        {
        }

        public IEnumerable<Bitmap> Download()
        {
            WiaException.Check(NativeWiaMethods.Download(Handle, 0, IntPtr.Zero, out var bytes));
            return null;
        }
    }
}