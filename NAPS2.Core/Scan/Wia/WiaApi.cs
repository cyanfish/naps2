/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2015       Phil Walter
    Copyright (C) 2012-2015  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NAPS2.Lang.Resources;
using NAPS2.Scan.Exceptions;
using NAPS2.Util;
using WIA;

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
            var wiaCommonDialog = new CommonDialogClass();
            try
            {
                Device d = wiaCommonDialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, true, false);
                if (d == null)
                {
                    return null;
                }
                return new ScanDevice(d.DeviceID, GetDeviceName(d.DeviceID));
            }
            catch (COMException e)
            {
                ThrowDeviceError(e);
                return null;
            }
        }

        public static List<ScanDevice> GetScanDeviceList()
        {
            DeviceManager manager = new DeviceManagerClass();

            var devices = new List<ScanDevice>();
            foreach (DeviceInfo info in manager.DeviceInfos)
            {
                Device device = info.Connect();
                devices.Add(new ScanDevice(info.DeviceID, GetDeviceName(device)));
            }
            return devices;
        }

        public static Device GetDevice(ScanDevice scanDevice)
        {
            DeviceManager manager = new DeviceManagerClass();
            foreach (DeviceInfo info in manager.DeviceInfos)
            {
                if (info.DeviceID == scanDevice.ID)
                {
                    try
                    {
                        return info.Connect();
                    }
                    catch (COMException e)
                    {
                        ThrowDeviceError(e);
                    }
                }
            }
            throw new DeviceNotFoundException();
        }

        public static string GetDeviceName(Device device)
        {
            return GetDeviceProperty(device, DeviceProperties.DEVICE_NAME);
        }

        public static string GetDeviceName(string deviceID)
        {
            DeviceManager manager = new DeviceManagerClass();
            foreach (DeviceInfo info in manager.DeviceInfos)
            {
                if (info.DeviceID == deviceID)
                {
                    Device device = info.Connect();
                    return GetDeviceName(device);
                }
            }
            throw new DeviceNotFoundException();
        }

        public static Item GetItem(Device device, ScanProfile profile)
        {
            Item item;
            if (profile.UseNativeUI)
            {
                try
                {
                    var items = new CommonDialogClass().ShowSelectItems(device, WiaImageIntent.UnspecifiedIntent,
                        WiaImageBias.MaximizeQuality, true, true, true);
                    item = items[1];
                }
                catch (COMException e)
                {
                    if ((uint)e.ErrorCode == Errors.UI_CANCELED)
                        return null;
                    throw;
                }
            }
            else
            {
                item = device.Items[1];
            }
            return item;
        }

        public static void ThrowDeviceError(Exception error)
        {
            if (error is ScanDriverException)
            {
                throw error;
            }
            if (error is COMException)
            {
                ThrowDeviceError((COMException)error);
            }
            throw new ScanDriverUnknownException(error);
        }

        public static void ThrowDeviceError(COMException e)
        {
            if ((uint)e.ErrorCode == Errors.NO_DEVICE_FOUND)
            {
                throw new NoDevicesFoundException();
            }
            if ((uint)e.ErrorCode == Errors.OUT_OF_PAPER)
            {
                throw new NoPagesException();
            }
            if ((uint)e.ErrorCode == Errors.OFFLINE)
            {
                throw new DeviceException(MiscResources.DeviceOffline);
            }
            if ((uint)e.ErrorCode == Errors.BUSY)
            {
                throw new DeviceException(MiscResources.DeviceBusy);
            }
            if ((uint)e.ErrorCode == Errors.COVER_OPEN)
            {
                throw new DeviceException(MiscResources.DeviceCoverOpen);
            }
            if ((uint)e.ErrorCode == Errors.PAPER_JAM)
            {
                throw new DeviceException(MiscResources.DevicePaperJam);
            }
            if ((uint)e.ErrorCode == Errors.WARMING_UP)
            {
                throw new DeviceException(MiscResources.DeviceWarmingUp);
            }
            throw new ScanDriverUnknownException(e);
        }

        #endregion

        #region Device/Item Configuration

        public static void Configure(Device device, Item item, ScanProfile profile)
        {
            if (profile.UseNativeUI)
            {
                return;
            }

            ConfigureDeviceProperties(device, profile);
            ConfigureItemProperties(device, item, profile);
        }

        private static void ConfigureItemProperties(Device device, Item item, ScanProfile profile)
        {
            switch (profile.BitDepth)
            {
                case ScanBitDepth.Grayscale:
                    SetItemIntProperty(item, 2, ItemProperties.DATA_TYPE);
                    break;
                case ScanBitDepth.C24Bit:
                    SetItemIntProperty(item, 3, ItemProperties.DATA_TYPE);
                    break;
                case ScanBitDepth.BlackWhite:
                    SetItemIntProperty(item, 0, ItemProperties.DATA_TYPE);
                    break;
            }

            int maxResolution = Math.Min(GetItemIntPropertyMax(item, ItemProperties.VERTICAL_RESOLUTION),
                GetItemIntPropertyMax(item, ItemProperties.HORIZONTAL_RESOLUTION));
            int resolution = Math.Min(profile.Resolution.ToIntDpi(), maxResolution);
            SetItemIntProperty(item, resolution, ItemProperties.VERTICAL_RESOLUTION);
            SetItemIntProperty(item, resolution, ItemProperties.HORIZONTAL_RESOLUTION);

            PageDimensions pageDimensions = profile.PageSize.PageDimensions() ?? profile.CustomPageSize;
            if (pageDimensions == null)
            {
                throw new InvalidOperationException("No page size specified");
            }
            int pageWidth = pageDimensions.WidthInThousandthsOfAnInch() * resolution / 1000;
            int pageHeight = pageDimensions.HeightInThousandthsOfAnInch() * resolution / 1000;

            int horizontalSize =
                GetDeviceIntProperty(device, profile.PaperSource == ScanSource.Glass
                    ? DeviceProperties.HORIZONTAL_BED_SIZE
                    : DeviceProperties.HORIZONTAL_FEED_SIZE);
            int verticalSize =
                GetDeviceIntProperty(device, profile.PaperSource == ScanSource.Glass
                    ? DeviceProperties.VERTICAL_BED_SIZE
                    : DeviceProperties.VERTICAL_FEED_SIZE);

            int pagemaxwidth = horizontalSize * resolution / 1000;
            int pagemaxheight = verticalSize * resolution / 1000;

            int horizontalPos = 0;
            if (profile.PageAlign == ScanHorizontalAlign.Center)
                horizontalPos = (pagemaxwidth - pageWidth) / 2;
            else if (profile.PageAlign == ScanHorizontalAlign.Left)
                horizontalPos = (pagemaxwidth - pageWidth);

            pageWidth = pageWidth < pagemaxwidth ? pageWidth : pagemaxwidth;
            pageHeight = pageHeight < pagemaxheight ? pageHeight : pagemaxheight;

            if (profile.WiaOffsetWidth)
            {
                SetItemIntProperty(item, pageWidth + horizontalPos, ItemProperties.HORIZONTAL_EXTENT);
                SetItemIntProperty(item, horizontalPos, ItemProperties.HORIZONTAL_START);
            }
            else
            {
                SetItemIntProperty(item, horizontalPos, ItemProperties.HORIZONTAL_START);
                SetItemIntProperty(item, pageWidth, ItemProperties.HORIZONTAL_EXTENT);
            }
            SetItemIntProperty(item, pageHeight, ItemProperties.VERTICAL_EXTENT);

            if (!profile.BrightnessContrastAfterScan)
            {
                SetItemIntProperty(item, profile.Contrast, -1000, 1000, ItemProperties.CONTRAST);
                SetItemIntProperty(item, profile.Brightness, -1000, 1000, ItemProperties.BRIGHTNESS);
            }
        }

        private static void ConfigureDeviceProperties(Device device, ScanProfile profile)
        {
            if (profile.PaperSource != ScanSource.Glass && DeviceSupportsFeeder(device))
            {
                SetDeviceIntProperty(device, 1, DeviceProperties.PAGES);
            }

            switch (profile.PaperSource)
            {
                case ScanSource.Glass:
                    SetDeviceIntProperty(device, Source.FLATBED, DeviceProperties.PAPER_SOURCE);
                    break;
                case ScanSource.Feeder:
                    SetDeviceIntProperty(device, Source.FEEDER, DeviceProperties.PAPER_SOURCE);
                    break;
                case ScanSource.Duplex:
                    SetDeviceIntProperty(device, Source.DUPLEX | Source.FEEDER, DeviceProperties.PAPER_SOURCE);
                    break;
            }
        }

        #endregion

        #region Derived Properties

        public static bool DeviceSupportsFeeder(Device device)
        {
            int capabilities = GetDeviceIntProperty(device, DeviceProperties.DOCUMENT_HANDLING_CAPABILITIES);
            return (capabilities & Source.FEEDER) != 0;
        }

        public static bool DeviceSupportsDuplex(Device device)
        {
            int capabilities = GetDeviceIntProperty(device, DeviceProperties.DOCUMENT_HANDLING_CAPABILITIES);
            return (capabilities & Source.DUPLEX) != 0;
        }

        public static bool DeviceFeederReady(Device device)
        {
            int status = GetDeviceIntProperty(device, DeviceProperties.DOCUMENT_HANDLING_STATUS);
            return (status & Status.FEED_READY) != 0;
        }

        #endregion

        #region WIA Property Getters and Setters

        private static string GetDeviceProperty(Device device, int propid)
        {
            foreach (Property property in device.Properties)
            {
                if (property.PropertyID == propid)
                {
                    return property.get_Value().ToString();
                }
            }
            return "";
        }

        private static int GetDeviceIntProperty(Device device, int propid)
        {
            foreach (Property property in device.Properties)
            {
                if (property.PropertyID == propid)
                {
                    return (int)property.get_Value();
                }
            }
            return 0;
        }

        private static void SetDeviceIntProperty(Device device, int value, int propid)
        {
            foreach (Property property in device.Properties)
            {
                if (property.PropertyID == propid)
                {
                    object objprop = value;
                    try
                    {
                        property.set_Value(ref objprop);
                    }
                    catch (ArgumentException)
                    {
                        // Ignore unsupported properties or value out of range
                    }
                }
            }
        }

        private static int GetItemIntPropertyMax(Item item, int propid)
        {
            try
            {
                foreach (Property property in item.Properties)
                {
                    if (property.PropertyID == propid)
                    {
                        if (property.SubType == WiaSubType.RangeSubType)
                        {
                            return property.SubTypeMax;
                        }
                        if (property.SubType == WiaSubType.ListSubType)
                        {
                            return property.SubTypeValues.Cast<int>().Max();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error getting property max", ex);
            }
            return int.MaxValue;
        }

        private static void SetItemIntProperty(Item item, int value, int propid)
        {
            foreach (Property property in item.Properties)
            {
                if (property.PropertyID == propid)
                {
                    object objprop = value;
                    try
                    {
                        property.set_Value(ref objprop);
                    }
                    catch (ArgumentException)
                    {
                        // Ignore unsupported properties or value out of range
                    }
                }
            }
        }

        private static void SetItemIntProperty(Item item, int value, int expectedMin, int expectedMax, int propid)
        {
            foreach (Property property in item.Properties)
            {
                if (property.PropertyID == propid)
                {
                    int expectedAbs = value - expectedMin;
                    int expectedRange = expectedMax - expectedMin;
                    int actualRange = property.SubTypeMax - property.SubTypeMin;
                    int actualValue = expectedAbs * actualRange / expectedRange + property.SubTypeMin;
                    if (property.SubTypeStep != 0)
                    {
                        actualValue -= actualValue % property.SubTypeStep;
                    }
                    actualValue = Math.Min(actualValue, property.SubTypeMax);
                    actualValue = Math.Max(actualValue, property.SubTypeMin);
                    object objprop = actualValue;
                    try
                    {
                        property.set_Value(ref objprop);
                    }
                    catch (ArgumentException)
                    {
                        // Ignore unsupported properties or value out of range
                    }
                }
            }
        }

        #endregion
    }
}
