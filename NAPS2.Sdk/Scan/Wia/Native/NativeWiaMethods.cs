using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using NAPS2.Platform;
using NAPS2.Util;

namespace NAPS2.Scan.Wia.Native
{
    internal static class NativeWiaMethods
    {
        static NativeWiaMethods()
        {
            const string lib64Dir = "64";
            if (Environment.Is64BitProcess && PlatformCompat.System.CanUseWin32)
            {
                var location = Assembly.GetExecutingAssembly().Location;
                var coreDllDir = System.IO.Path.GetDirectoryName(location);
                if (coreDllDir != null)
                {
                    Win32.SetDllDirectory(System.IO.Path.Combine(coreDllDir, lib64Dir));
                }
            }
        }

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
        public static extern uint GetPropertyAttributes(
            IntPtr propStorage,
            int propId,
            [Out] out int flags,
            [Out] out int min,
            [Out] out int nom,
            [Out] out int max,
            [Out] out int step,
            [Out] out int numElems,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 7), Out] out int[] elems);
        
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

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetImage1(IntPtr deviceManager, IntPtr hwnd, int deviceType, int flags, int intent, [MarshalAs(UnmanagedType.BStr)] string filePath, IntPtr item);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint GetImage2(
            IntPtr deviceManager,
            int flags,
            [MarshalAs(UnmanagedType.BStr)] string deviceId,
            IntPtr hwnd,
            [MarshalAs(UnmanagedType.BStr)] string folder,
            [MarshalAs(UnmanagedType.BStr)] string fileName, [In, Out] ref int numFiles,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 6, ArraySubType = UnmanagedType.BStr), In, Out] ref string[] filePaths,
            [In, Out] ref IntPtr item);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint ConfigureDevice1(IntPtr device, IntPtr hwnd, int flags, int intent, [In, Out] ref int itemCount, [In, Out] ref IntPtr[] items);

        [DllImport("NAPS2.WIA.dll")]
        public static extern uint ConfigureDevice2(
            IntPtr device,
            int flags,
            IntPtr hwnd,
            [MarshalAs(UnmanagedType.BStr)] string folder,
            [MarshalAs(UnmanagedType.BStr)] string fileName,
            [In, Out] ref int numFiles,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5, ArraySubType = UnmanagedType.BStr), In, Out] ref string[] filePaths,
            IntPtr[] items);
    }
}
