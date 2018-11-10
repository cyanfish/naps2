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
        public static extern uint GetDeviceManager1([Out] out IntPtr deviceManager);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetDeviceManager2([Out] out IntPtr deviceManager);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetDevice1(IntPtr deviceManager, [MarshalAs(UnmanagedType.BStr)] string deviceId, [Out] out IntPtr device);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetDevice2(IntPtr deviceManager, [MarshalAs(UnmanagedType.BStr)] string deviceId, [Out] out IntPtr device);

        public delegate void EnumDeviceCallback(IntPtr devicePropStorage);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint EnumerateDevices1(IntPtr deviceManager, [MarshalAs(UnmanagedType.FunctionPtr)] EnumDeviceCallback func);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint EnumerateDevices2(IntPtr deviceManager, [MarshalAs(UnmanagedType.FunctionPtr)] EnumDeviceCallback func);

        public delegate void EnumItemCallback(IntPtr item);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint EnumerateItems1(IntPtr item, [MarshalAs(UnmanagedType.FunctionPtr)] EnumItemCallback func);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint EnumerateItems2(IntPtr item, [MarshalAs(UnmanagedType.FunctionPtr)] EnumItemCallback func);

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
        public static extern uint StartTransfer1(IntPtr item, [Out] out IntPtr transfer);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint StartTransfer2(IntPtr item, [Out] out IntPtr transfer);

        public delegate bool TransferStatusCallback(int msgType, int percent, ulong bytesTransferred, uint hresult, [MarshalAs(UnmanagedType.Interface)] IStream stream);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint Download1(IntPtr transfer, [MarshalAs(UnmanagedType.FunctionPtr)] TransferStatusCallback func);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint Download2(IntPtr transfer, [MarshalAs(UnmanagedType.FunctionPtr)] TransferStatusCallback func);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint SelectDevice1(IntPtr deviceManager, IntPtr hwnd, int deviceType, int flags, [MarshalAs(UnmanagedType.BStr), Out] out string deviceId, [Out] out IntPtr device);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint SelectDevice2(IntPtr deviceManager, IntPtr hwnd, int deviceType, int flags, [MarshalAs(UnmanagedType.BStr), Out] out string deviceId, [Out] out IntPtr device);
    }
}
