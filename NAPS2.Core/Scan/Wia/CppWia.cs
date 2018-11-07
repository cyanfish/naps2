using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.Scan.Wia
{
    public static class CppWia
    {
        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetDeviceManager([Out] out IntPtr deviceManager);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetDevice(IntPtr deviceManager, [MarshalAs(UnmanagedType.BStr)] string deviceId, [Out] out IntPtr device);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint SetDeviceProperty(IntPtr device, int propId, int value);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetItem(IntPtr device, [MarshalAs(UnmanagedType.BStr)] string itemId, [Out] out IntPtr item);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint SetItemProperty(IntPtr item, int propId, int value);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint StartTransfer(IntPtr item, [Out] out IntPtr transfer);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint Download(IntPtr transfer, int flags, IntPtr callbackTodo, [Out] out byte[] bytes);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint EndTransfer(IntPtr transfer);
    }
}
