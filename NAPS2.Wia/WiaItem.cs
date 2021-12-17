using System;

namespace NAPS2.Wia;

public class WiaItem : WiaItemBase
{
    protected internal WiaItem(WiaVersion version, IntPtr handle) : base(version, handle)
    {
    }
        
    public WiaTransfer StartTransfer()
    {
        WiaException.Check(Version == WiaVersion.Wia10
            ? NativeWiaMethods.StartTransfer1(Handle, out var transferHandle)
            : NativeWiaMethods.StartTransfer2(Handle, out transferHandle));
        return new WiaTransfer(Version, transferHandle);
    }
}