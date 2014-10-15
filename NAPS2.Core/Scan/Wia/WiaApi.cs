/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

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
        private const int DEV_NAME = 7;
        private const int HORIZONTAL_FEED_SIZE = 3076;
        private const int VERTICAL_FEED_SIZE = 3077;
        private const int HORIZONTAL_BED_SIZE = 3074;
        private const int VERTICAL_BED_SIZE = 3075;
        private const int PAPER_SOURCE = 3088;
        private const int DATA_TYPE = 4103;
        private const int VERTICAL_RESOLUTION = 6148;
        private const int HORIZONTAL_RESOLUTION = 6147;
        private const int HORIZONTAL_EXTENT = 6151;
        private const int VERTICAL_EXTENT = 6152;
        private const int BRIGHTNESS = 6154;
        private const int CONTRAST = 6155;
        private const int HORIZONTAL_START = 6149;

        private const int SOURCE_FEEDER = 1;
        private const int SOURCE_FLATBED = 2;
        private const int SOURCE_DUPLEX = 4;

        private const uint ERROR_OUT_OF_PAPER = 0x80210003;
        private const uint NO_DEVICE_FOUND = 0x80210015;
        private const uint ERROR_OFFLINE = 0x80210005;
        private const uint PAPER_JAM = 0x8021000A;

        private const uint UI_CANCELED = 0x80210064;

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
                        if ((uint)e.ErrorCode == ERROR_OFFLINE)
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
            get { return GetDeviceProperty(DEV_NAME); }
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
                if ((uint)e.ErrorCode == NO_DEVICE_FOUND)
                {
                    throw new NoDevicesFoundException();
                }
                if ((uint)e.ErrorCode == ERROR_OFFLINE)
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
                    return GetDeviceProperty(device, DEV_NAME);
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
            object objprop = value;
            foreach (Property property in device.Properties)
            {
                if (property.PropertyID == propid)
                {
                    property.set_Value(ref objprop);
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
                        // Ignore unsupported properties (e.g. contrast)
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
                        // Ignore unsupported properties (e.g. contrast)
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
                    SetItemIntProperty(item, 2, DATA_TYPE);
                    break;
                case ScanBitDepth.C24Bit:
                    SetItemIntProperty(item, 3, DATA_TYPE);
                    break;
                case ScanBitDepth.BlackWhite:
                    SetItemIntProperty(item, 0, DATA_TYPE);
                    break;
            }

            switch (settings.Resolution)
            {
                case ScanDpi.Dpi100:
                    SetItemIntProperty(item, 100, VERTICAL_RESOLUTION);
                    SetItemIntProperty(item, 100, HORIZONTAL_RESOLUTION);
                    resolution = 100;
                    break;
                case ScanDpi.Dpi150:
                    SetItemIntProperty(item, 150, VERTICAL_RESOLUTION);
                    SetItemIntProperty(item, 150, HORIZONTAL_RESOLUTION);
                    resolution = 150;
                    break;
                case ScanDpi.Dpi200:
                    SetItemIntProperty(item, 200, VERTICAL_RESOLUTION);
                    SetItemIntProperty(item, 200, HORIZONTAL_RESOLUTION);
                    resolution = 200;
                    break;
                case ScanDpi.Dpi300:
                    SetItemIntProperty(item, 300, VERTICAL_RESOLUTION);
                    SetItemIntProperty(item, 300, HORIZONTAL_RESOLUTION);
                    resolution = 300;
                    break;
                case ScanDpi.Dpi600:
                    SetItemIntProperty(item, 600, VERTICAL_RESOLUTION);
                    SetItemIntProperty(item, 600, HORIZONTAL_RESOLUTION);
                    resolution = 600;
                    break;
                case ScanDpi.Dpi1200:
                    SetItemIntProperty(item, 1200, VERTICAL_RESOLUTION);
                    SetItemIntProperty(item, 1200, HORIZONTAL_RESOLUTION);
                    resolution = 120;
                    break;
            }

            Size pageSize = settings.PageSize.ToSize();
            int pageWidth = pageSize.Width * resolution / 1000;
            int pageHeight = pageSize.Height * resolution / 1000;
            int horizontalSize = GetDeviceIntProperty(settings.PaperSource == ScanSource.Glass ? HORIZONTAL_BED_SIZE : HORIZONTAL_FEED_SIZE);

            int verticalSize = GetDeviceIntProperty(settings.PaperSource == ScanSource.Glass ? VERTICAL_BED_SIZE : VERTICAL_FEED_SIZE);

            int pagemaxwidth = horizontalSize * resolution / 1000;
            int pagemaxheight = verticalSize * resolution / 1000;

            int horizontalPos = 0;
            if (settings.PageAlign == ScanHorizontalAlign.Center)
                horizontalPos = (pagemaxwidth - pageWidth) / 2;
            else if (settings.PageAlign == ScanHorizontalAlign.Left)
                horizontalPos = (pagemaxwidth - pageWidth);

            pageWidth = pageWidth < pagemaxwidth ? pageWidth : pagemaxwidth;
            pageHeight = pageHeight < pagemaxheight ? pageHeight : pagemaxheight;

            SetItemIntProperty(item, pageWidth, HORIZONTAL_EXTENT);
            SetItemIntProperty(item, pageHeight, VERTICAL_EXTENT);
            SetItemIntProperty(item, horizontalPos, HORIZONTAL_START);
            SetItemIntProperty(item, settings.Contrast, -1000, 1000, CONTRAST);
            SetItemIntProperty(item, settings.Brightness, -1000, 1000, BRIGHTNESS);
        }

        private void SetupDevice()
        {
            switch (settings.PaperSource)
            {
                case ScanSource.Glass:
                    SetDeviceIntProperty(SOURCE_FLATBED, PAPER_SOURCE);
                    break;
                case ScanSource.Feeder:
                    SetDeviceIntProperty(SOURCE_FEEDER, PAPER_SOURCE);
                    break;
                case ScanSource.Duplex:
                    SetDeviceIntProperty(SOURCE_DUPLEX | SOURCE_FEEDER, PAPER_SOURCE);
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

        public IScannedImage GetImage()
        {
            try
            {
                var wiaCommonDialog = new CommonDialogClass();

                Items items = device.Items;
                if (settings.UseNativeUI)
                {
                    try
                    {
                        items = wiaCommonDialog.ShowSelectItems(device, WiaImageIntent.UnspecifiedIntent,
                            WiaImageBias.MaximizeQuality, true, true, true);
                    }
                    catch (COMException e)
                    {
                        if ((uint)e.ErrorCode == UI_CANCELED)
                            return null;
                    }
                }
                else
                {
                    SetupDevice();
                    SetupItem(items[1]);
                }
                var file =
                    (ImageFile)wiaCommonDialog.ShowTransfer(items[1], "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}", false);
                if (file == null)
                {
                    // User cancelled
                    return null;
                }

                using (var stream = new MemoryStream((byte[])file.FileData.get_BinaryData()))
                {
                    using (Image output = Image.FromStream(stream))
                    {

                        double koef = 1;

                        if (!settings.UseNativeUI)
                        {
                            switch (settings.AfterScanScale)
                            {
                                case ScanScale.OneToOne:
                                    koef = 1;
                                    break;
                                case ScanScale.OneToTwo:
                                    koef = 2;
                                    break;
                                case ScanScale.OneToFour:
                                    koef = 4;
                                    break;
                                case ScanScale.OneToEight:
                                    koef = 8;
                                    break;
                            }
                        }

                        double realWidth = output.Width / koef;
                        double realHeight = output.Height / koef;

                        double horizontalRes = output.HorizontalResolution / koef;
                        double verticalRes = output.VerticalResolution / koef;

                        using (var result = new Bitmap((int)realWidth, (int)realHeight))
                        using (Graphics g = Graphics.FromImage(result))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.DrawImage(output, 0, 0, (int)realWidth, (int)realHeight);

                            result.SetResolution((float)horizontalRes, (float)verticalRes);

                            ScanBitDepth bitDepth = settings.UseNativeUI ? ScanBitDepth.C24Bit : settings.BitDepth;
                            return scannedImageFactory.Create(result, bitDepth, settings.MaxQuality);
                        }
                    }
                }
            }
            catch (COMException e)
            {
                if ((uint)e.ErrorCode == ERROR_OUT_OF_PAPER)
                {
                    return null;
                }
                else if ((uint)e.ErrorCode == ERROR_OFFLINE)
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
