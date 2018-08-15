// See WiaApi.cs for an explanation of this guard
#if NONWINDOWS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NAPS2.Lang.Resources;
using NAPS2.Scan.Exceptions;
using NAPS2.Util;

namespace NAPS2.Scan.Wia
{
    internal static class WiaApi
    {
        #region WIA Constants

        public static class DeviceProperties
        {
            public const int DEVICE_NAME = 7;
            public const int HORIZONTAL_FEED_SIZE = 3076;
            public const int VERTICAL_FEED_SIZE = 3077;
            public const int HORIZONTAL_BED_SIZE = 3074;
            public const int VERTICAL_BED_SIZE = 3075;
            public const int PAPER_SOURCE = 3088;
            public const int DOCUMENT_HANDLING_CAPABILITIES = 3086;
            public const int DOCUMENT_HANDLING_STATUS = 3087;
            public const int PAGES = 3096;
        }

        public static class ItemProperties
        {
            public const int DATA_TYPE = 4103;
            public const int VERTICAL_RESOLUTION = 6148;
            public const int HORIZONTAL_RESOLUTION = 6147;
            public const int HORIZONTAL_EXTENT = 6151;
            public const int VERTICAL_EXTENT = 6152;
            public const int BRIGHTNESS = 6154;
            public const int CONTRAST = 6155;
            public const int HORIZONTAL_START = 6149;
        }

        public static class Errors
        {
            public const uint OUT_OF_PAPER = 0x80210003;
            public const uint NO_DEVICE_FOUND = 0x80210015;
            public const uint OFFLINE = 0x80210005;
            public const uint PAPER_JAM = 0x80210002;
            public const uint BUSY = 0x80210006;
            public const uint COVER_OPEN = 0x80210016;
            public const uint COMMUNICATION = 0x8021000A;
            public const uint LOCKED = 0x8021000D;
            public const uint INCORRECT_SETTING = 0x8021000C;
            public const uint LAMP_OFF = 0x80210017;
            public const uint WARMING_UP = 0x80210007;

            public const uint UI_CANCELED = 0x80210064;
        }

        public static class Source
        {
            public const int FEEDER = 1;
            public const int FLATBED = 2;
            public const int DUPLEX = 4;
        }

        public static class Status
        {
            public const int FEED_READY = 1;
        }

        public static class Formats
        {
            public const string BMP = "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}";
        }

        #endregion

        #region Device/Item Management

        public static ScanDevice PromptForScanDevice()
        {
            throw new NotImplementedException();
        }

        public static List<ScanDevice> GetScanDeviceList()
        {
            throw new NotImplementedException();
        }

        public static object GetDevice(ScanDevice scanDevice)
        {
            throw new NotImplementedException();
        }

        public static string GetDeviceName(object device)
        {
            throw new NotImplementedException();
        }

        public static string GetDeviceName(string deviceID)
        {
            throw new NotImplementedException();
        }

        public static object GetItem(object device, ScanProfile profile)
        {
            throw new NotImplementedException();
        }

        public static void ThrowDeviceError(Exception error)
        {
            throw new NotImplementedException();
        }

        public static void ThrowDeviceError(COMException e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Device/Item Configuration

        public static void Configure(object device, object item, ScanProfile profile)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Derived Properties

        public static bool DeviceSupportsFeeder(object device)
        {
            throw new NotImplementedException();
        }

        public static bool DeviceSupportsDuplex(object device)
        {
            throw new NotImplementedException();
        }

        public static bool DeviceFeederReady(object device)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region WIA Property Getters and Setters
        
        #endregion

        #region Scanning

        public static Stream Transfer(WiaState wia, string format, bool showGui)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

#endif
