using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace NAPS2.Scan.Wia.Native
{
    internal static class NativeWiaMethods
    {
        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetDeviceManager([Out] out IntPtr deviceManager);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetDevice(IntPtr deviceManager, [MarshalAs(UnmanagedType.BStr)] string deviceId, [Out] out IntPtr device);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetItem(IntPtr device, [MarshalAs(UnmanagedType.BStr)] string itemId, [Out] out IntPtr item);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint SetItemProperty(IntPtr item, int propId, int value);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint StartTransfer(IntPtr item, [Out] out IntPtr transfer);

        public delegate void TransferStatusCallback(int msgType, int percent, ulong bytesTransferred, uint hresult, [MarshalAs(UnmanagedType.Interface)] IStream stream);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint Download(IntPtr transfer, int flags, [MarshalAs(UnmanagedType.FunctionPtr)] TransferStatusCallback func);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint CancelTransfer(IntPtr transfer);
    }
}
