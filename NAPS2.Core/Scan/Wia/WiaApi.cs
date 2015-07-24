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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using WIA;

namespace NAPS2.Scan.Wia
{
    internal class WiaApi
    {
        #region WIA Constants

        private static class DeviceProperties
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

        private static class ItemProperties
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

        private static class Errors
        {
            public const uint OUT_OF_PAPER = 0x80210003;
            public const uint NO_DEVICE_FOUND = 0x80210015;
            public const uint OFFLINE = 0x80210005;
            public const uint PAPER_JAM = 0x8021000A;

            public const uint UI_CANCELED = 0x80210064;
        }

        private static class Source
        {
            public const int FEEDER = 1;
            public const int FLATBED = 2;
            public const int DUPLEX = 4;
        }

        private static class Status
        {
            public const int FEED_READY = 1;
        }

        private static class Formats
        {
            public const string BMP = "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}";
        }

        #endregion

        private readonly Device device;

        private readonly ExtendedScanSettings settings;
        private readonly IScannedImageFactory scannedImageFactory;

        public WiaApi(ExtendedScanSettings settings, ScanDevice scanDevice, IScannedImageFactory scannedImageFactory)
        {
            this.settings = settings;
            this.scannedImageFactory = scannedImageFactory;
            DeviceManager manager = new DeviceManagerClass();
            foreach (DeviceInfo info in manager.DeviceInfos)
            {
                if (info.DeviceID == scanDevice.ID)
                {
                    try
                    {
                        device = info.Connect();
                    }
                    catch (COMException e)
                    {
                        if ((uint)e.ErrorCode == Errors.OFFLINE)
                        {
                            throw new DeviceOfflineException();
                        }
                        throw new ScanDriverUnknownException(e);
                    }
                    return;
                }
            }
            throw new DeviceNotFoundException();
        }

        public string DeviceName
        {
            get { return GetDeviceProperty(DeviceProperties.DEVICE_NAME); }
        }

        public static ScanDevice SelectDeviceUI()
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
                if ((uint)e.ErrorCode == Errors.NO_DEVICE_FOUND)
                {
                    throw new NoDevicesFoundException();
                }
                if ((uint)e.ErrorCode == Errors.OFFLINE)
                {
                    throw new DeviceOfflineException();
                }
                throw new ScanDriverUnknownException(e);
            }
        }

        public static string GetDeviceName(string deviceID)
        {
            DeviceManager manager = new DeviceManagerClass();
            foreach (DeviceInfo info in manager.DeviceInfos)
            {
                if (info.DeviceID == deviceID)
                {
                    Device device = info.Connect();
                    return GetDeviceProperty(device, DeviceProperties.DEVICE_NAME);
                }
            }
            throw new DeviceNotFoundException();
        }

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

        private string GetDeviceProperty(int propid)
        {
            return GetDeviceProperty(device, propid);
        }

        private int GetDeviceIntProperty(int propid)
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

        private void SetDeviceIntProperty(int value, int propid)
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

        private void SetItemIntProperty(Item item, int value, int propid)
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

        private void SetItemIntProperty(Item item, int value, int expectedMin, int expectedMax, int propid)
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

        private void SetupItem(Item item)
        {
            int resolution = 0;
            switch (settings.BitDepth)
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

            resolution = settings.Resolution.ToIntDpi();
            SetItemIntProperty(item, resolution, ItemProperties.VERTICAL_RESOLUTION);
            SetItemIntProperty(item, resolution, ItemProperties.HORIZONTAL_RESOLUTION);

            PageDimensions pageDimensions = settings.PageSize.PageDimensions() ?? settings.CustomPageSize;
            if (pageDimensions == null)
            {
                throw new InvalidOperationException("No page size specified");
            }
            int pageWidth = pageDimensions.WidthInThousandthsOfAnInch() * resolution / 1000;
            int pageHeight = pageDimensions.HeightInThousandthsOfAnInch() * resolution / 1000;

            int horizontalSize = GetDeviceIntProperty(settings.PaperSource == ScanSource.Glass ? DeviceProperties.HORIZONTAL_BED_SIZE : DeviceProperties.HORIZONTAL_FEED_SIZE);
            int verticalSize = GetDeviceIntProperty(settings.PaperSource == ScanSource.Glass ? DeviceProperties.VERTICAL_BED_SIZE : DeviceProperties.VERTICAL_FEED_SIZE);

            int pagemaxwidth = horizontalSize * resolution / 1000;
            int pagemaxheight = verticalSize * resolution / 1000;

            int horizontalPos = 0;
            if (settings.PageAlign == ScanHorizontalAlign.Center)
                horizontalPos = (pagemaxwidth - pageWidth) / 2;
            else if (settings.PageAlign == ScanHorizontalAlign.Left)
                horizontalPos = (pagemaxwidth - pageWidth);

            pageWidth = pageWidth < pagemaxwidth ? pageWidth : pagemaxwidth;
            pageHeight = pageHeight < pagemaxheight ? pageHeight : pagemaxheight;

            SetItemIntProperty(item, pageWidth, ItemProperties.HORIZONTAL_EXTENT);
            SetItemIntProperty(item, pageHeight, ItemProperties.VERTICAL_EXTENT);
            SetItemIntProperty(item, horizontalPos, ItemProperties.HORIZONTAL_START);
            SetItemIntProperty(item, settings.Contrast, -1000, 1000, ItemProperties.CONTRAST);
            SetItemIntProperty(item, settings.Brightness, -1000, 1000, ItemProperties.BRIGHTNESS);
        }

        public bool SupportsFeeder
        {
            get
            {
                int capabilities = GetDeviceIntProperty(DeviceProperties.DOCUMENT_HANDLING_CAPABILITIES);
                return (capabilities & Source.FEEDER) != 0;
            }
        }

        public bool FeederReady
        {
            get
            {
                int status = GetDeviceIntProperty(DeviceProperties.DOCUMENT_HANDLING_STATUS);
                return (status & Status.FEED_READY) != 0;
            }
        }

        private void SetupDevice()
        {
            if (settings.PaperSource != ScanSource.Glass && SupportsFeeder)
            {
                SetDeviceIntProperty(1, DeviceProperties.PAGES);
            }

            switch (settings.PaperSource)
            {
                case ScanSource.Glass:
                    SetDeviceIntProperty(Source.FLATBED, DeviceProperties.PAPER_SOURCE);
                    break;
                case ScanSource.Feeder:
                    SetDeviceIntProperty(Source.FEEDER, DeviceProperties.PAPER_SOURCE);
                    break;
                case ScanSource.Duplex:
                    SetDeviceIntProperty(Source.DUPLEX | Source.FEEDER, DeviceProperties.PAPER_SOURCE);
                    break;
            }
        }

        /*private void EnumerateOptions()
        {
            foreach (Property prop in device.Properties)
            {
                if (!prop.IsReadOnly)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("DEV-{0}:{1}={2}", prop.PropertyID, prop.Name, prop.get_Value()));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("(RO)DEV-{0}:{1}={2}", prop.PropertyID, prop.Name, prop.get_Value()));
                }
            }
            foreach (Property prop in items[1].Properties)
            {
                if (!prop.IsReadOnly)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("IT-{0}:{1}={2}", prop.PropertyID, prop.Name, prop.get_Value()));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("(RO)IT-{0}:{1}={2}", prop.PropertyID, prop.Name, prop.get_Value()));
                }
            }
        }*/

        public IScannedImage GetImage(IWiaTransfer wiaTransfer, int pageNumber)
        {
            try
            {
                Items items = device.Items;
                if (settings.UseNativeUI)
                {
                    try
                    {
                        items = new CommonDialogClass().ShowSelectItems(device, WiaImageIntent.UnspecifiedIntent,
                            WiaImageBias.MaximizeQuality, true, true, true);
                    }
                    catch (COMException e)
                    {
                        if ((uint)e.ErrorCode == Errors.UI_CANCELED)
                            return null;
                    }
                }
                else
                {
                    SetupDevice();
                    SetupItem(items[1]);
                }
                var file = wiaTransfer.Transfer(pageNumber, device, items[1], Formats.BMP);
                if (file == null)
                {
                    // User cancelled
                    return null;
                }

                using (var stream = new MemoryStream((byte[])file.FileData.get_BinaryData()))
                {
                    using (Image output = Image.FromStream(stream))
                    {
                        double scaleFactor = 1;
                        if (!settings.UseNativeUI)
                        {
                            scaleFactor = settings.AfterScanScale.ToIntScaleFactor();
                        }

                        using (var result = TransformationHelper.ScaleImage(output, scaleFactor))
                        {
                            ScanBitDepth bitDepth = settings.UseNativeUI ? ScanBitDepth.C24Bit : settings.BitDepth;
                            return scannedImageFactory.Create(result, bitDepth, settings.MaxQuality);
                        }
                    }
                }
            }
            catch (COMException e)
            {
                if ((uint)e.ErrorCode == Errors.OUT_OF_PAPER)
                {
                    return null;
                }
                else if ((uint)e.ErrorCode == Errors.OFFLINE)
                {
                    throw new DeviceOfflineException();
                }
                else
                {
                    throw new ScanDriverUnknownException(e);
                }
            }
        }
    }
}
