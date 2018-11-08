using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaItem : WiaItemBase, IWiaItemProps
    {
        protected internal WiaItem(IntPtr handle) : base(handle)
        {
        }
        
        public WiaTransfer StartTransfer()
        {
            WiaException.Check(NativeWiaMethods.StartTransfer(Handle, out var transferHandle));
            return new WiaTransfer(transferHandle);
        }
    }
}