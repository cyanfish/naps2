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

        public delegate void EnumDeviceCallback(IntPtr devicePropStorage);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint EnumerateDevices(IntPtr deviceManager, [MarshalAs(UnmanagedType.FunctionPtr)] EnumDeviceCallback func);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetItem(IntPtr device, [MarshalAs(UnmanagedType.BStr)] string itemId, [Out] out IntPtr item);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetItemPropertyStorage(IntPtr item, out IntPtr propStorage);

        public delegate void EnumPropertyCallback(int propId, [MarshalAs(UnmanagedType.LPWStr)] string propName, ushort propType);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint EnumerateProperties(IntPtr propStorage, [MarshalAs(UnmanagedType.FunctionPtr)] EnumPropertyCallback func);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetPropertyBstr(IntPtr propStorage, int propId, [MarshalAs(UnmanagedType.BStr), Out] out string value);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetPropertyInt(IntPtr propStorage, int propId, [Out] out int value);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint SetPropertyInt(IntPtr propStorage, int propId, int value);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint StartTransfer(IntPtr item, [Out] out IntPtr transfer);

        public delegate void TransferStatusCallback(int msgType, int percent, ulong bytesTransferred, uint hresult, [MarshalAs(UnmanagedType.Interface)] IStream stream);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint Download(IntPtr transfer, int flags, [MarshalAs(UnmanagedType.FunctionPtr)] TransferStatusCallback func);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint CancelTransfer(IntPtr transfer);
    }
}
