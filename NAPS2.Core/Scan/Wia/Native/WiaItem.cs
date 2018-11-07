using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaItem : NativeWiaObject
    {
        protected internal WiaItem(IntPtr handle) : base(handle)
        {
        }

        public IEnumerable<WiaItem> GetSubItems()
        {
            return null;
        }

        public WiaTransfer StartTransfer()
        {
            WiaException.Check(NativeWiaMethods.StartTransfer(Handle, out var transferHandle));
            return new WiaTransfer(transferHandle);
        }

        public WiaItem FindSubItem(string name)
        {
            WiaException.Check(NativeWiaMethods.GetItem(Handle, name, out var itemHandle));
            return new WiaItem(itemHandle);
        }
    }
}