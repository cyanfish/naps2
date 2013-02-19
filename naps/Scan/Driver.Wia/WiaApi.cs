/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009        Pavel Sorejs
    Copyright (C) 2012, 2013  Ben Olden-Cooligan

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
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using WIA;

using NAPS2.Scan;

namespace NAPS2.Scan.Driver.Wia
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

        private const uint ERROR_OUT_OF_PAPER = 0x80210003;
        private const uint NO_DEVICE_FOUND = 0x80210015;
        private const uint ERROR_OFFLINE = 0x80210005;
        private const uint PAPER_JAM = 0x8021000A;

        private const uint UI_CANCELED = 0x80210064;

        private Device device;

        private ScanSettings settings;
        private ExtendedScanSettings settingsExt;

        public static ScanDevice SelectDeviceUI()
        {
            CommonDialogClass WIACommonDialog = new CommonDialogClass();
            try
            {
                Device d = WIACommonDialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, true, false);
                if (d == null)
                {
                    return null;
                }
                return new ScanDevice(d.DeviceID, GetDeviceName(d.DeviceID), WiaScanDriver.DRIVER_NAME);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                if ((uint)e.ErrorCode == NO_DEVICE_FOUND)
                {
                    throw new NoDevicesFoundException();
                }
                else if ((uint)e.ErrorCode == ERROR_OFFLINE)
                {
                    throw new DeviceOfflineException();
                }
                else
                {
                    throw new ScanDriverException(e);
                }
            }
        }

        public static string GetDeviceName(string deviceID)
        {
            DeviceManager manager = new DeviceManagerClass();
            foreach (DeviceInfo info in manager.DeviceInfos)
            {
                if (info.DeviceID == deviceID)
                {
                    var device = info.Connect();
                    return getDeviceProperty(device, DEV_NAME);
                }
            }
            throw new DeviceNotFoundException();
        }

        private static string getDeviceProperty(Device device, int propid)
        {
            foreach (WIA.Property property in device.Properties)
            {
                if (property.PropertyID == propid)
                {
                    return property.get_Value().ToString();
                }
            }
            return "";
        }

        public WiaApi(ScanSettings settings)
        {
            this.settings = settings;
            this.settingsExt = settings as ExtendedScanSettings;
            DeviceManager manager = new DeviceManagerClass();
            foreach (DeviceInfo info in manager.DeviceInfos)
            {
                if (info.DeviceID == settings.Device.ID)
                {
                    device = info.Connect();
                    return;
                }
            }
            throw new DeviceNotFoundException();
        }

        private string getDeviceProperty(int propid)
        {
            return getDeviceProperty(device, propid);
        }

        private int getDeviceIntProperty(int propid)
        {
            foreach (WIA.Property property in device.Properties)
            {
                if (property.PropertyID == propid)
                {
                    return (int)property.get_Value();
                }
            }
            return 0;
        }

        private void setDeviceIntProperty(int value, int propid)
        {
            object objprop = value;
            foreach (WIA.Property property in device.Properties)
            {
                if (property.PropertyID == propid)
                {
                    property.set_Value(ref objprop);
                }
            }
        }

        private void setItemIntProperty(Item item, int value, int propid)
        {
            foreach (WIA.Property property in item.Properties)
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

        private void setItemIntProperty(Item item, int value, int expectedMin, int expectedMax, int propid)
        {
            foreach (WIA.Property property in item.Properties)
            {
                if (property.PropertyID == propid)
                {
                    int expectedAbs = value - expectedMin;
                    int expectedRange = expectedMax - expectedMin;
                    int actualRange = property.SubTypeMax - property.SubTypeMin;
                    int actualValue = expectedAbs * actualRange / expectedRange + property.SubTypeMin;
                    actualValue -= actualValue % property.SubTypeStep;
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

        public string DeviceName
        {
            get { return getDeviceProperty(DEV_NAME); }
        }

        private void setupItem(Item item)
        {
            int resolution = 0;
            switch (settingsExt.BitDepth)
            {
                case ScanBitDepth.GRAYSCALE:
                    setItemIntProperty(item, 2, DATA_TYPE);
                    break;
                case ScanBitDepth.C24BIT:
                    setItemIntProperty(item, 3, DATA_TYPE);
                    break;
                case ScanBitDepth.BLACKWHITE:
                    setItemIntProperty(item, 0, DATA_TYPE);
                    break;
            }

            switch (settingsExt.Resolution)
            {
                case ScanDPI.DPI100:
                    setItemIntProperty(item, 100, VERTICAL_RESOLUTION);
                    setItemIntProperty(item, 100, HORIZONTAL_RESOLUTION);
                    resolution = 100;
                    break;
                case ScanDPI.DPI200:
                    setItemIntProperty(item, 200, VERTICAL_RESOLUTION);
                    setItemIntProperty(item, 200, HORIZONTAL_RESOLUTION);
                    resolution = 200;
                    break;
                case ScanDPI.DPI300:
                    setItemIntProperty(item, 300, VERTICAL_RESOLUTION);
                    setItemIntProperty(item, 300, HORIZONTAL_RESOLUTION);
                    resolution = 300;
                    break;
                case ScanDPI.DPI600:
                    setItemIntProperty(item, 600, VERTICAL_RESOLUTION);
                    setItemIntProperty(item, 600, HORIZONTAL_RESOLUTION);
                    resolution = 600;
                    break;
                case ScanDPI.DPI1200:
                    setItemIntProperty(item, 1200, VERTICAL_RESOLUTION);
                    setItemIntProperty(item, 1200, HORIZONTAL_RESOLUTION);
                    resolution = 120;
                    break;
            }

            Size pageSize = settingsExt.PageSize.ToSize();
            int pageWidth = pageSize.Width * resolution / 1000;
            int pageHeight = pageSize.Height * resolution / 1000;
            int horizontalSize = 0;
            if (settingsExt.Source == ScanSource.GLASS)
                horizontalSize = getDeviceIntProperty(HORIZONTAL_BED_SIZE);
            else
                horizontalSize = getDeviceIntProperty(HORIZONTAL_FEED_SIZE);

            int verticalSize = 0;
            if (settingsExt.Source == ScanSource.GLASS)
                verticalSize = getDeviceIntProperty(VERTICAL_BED_SIZE);
            else
                verticalSize = getDeviceIntProperty(VERTICAL_FEED_SIZE);

            int pagemaxwidth = horizontalSize * resolution / 1000;
            int pagemaxheight = verticalSize * resolution / 1000;


            int horizontalPos = 0;
            if (settingsExt.PageAlign == ScanHorizontalAlign.CENTER)
                horizontalPos = (pagemaxwidth - pageWidth) / 2;
            else if (settingsExt.PageAlign == ScanHorizontalAlign.LEFT)
                horizontalPos = (pagemaxwidth - pageWidth);

            pageWidth = pageWidth < pagemaxwidth ? pageWidth : pagemaxwidth;
            pageHeight = pageHeight < pagemaxheight ? pageHeight : pagemaxheight;

            setItemIntProperty(item, pageWidth, HORIZONTAL_EXTENT);
            setItemIntProperty(item, pageHeight, VERTICAL_EXTENT);
            setItemIntProperty(item, horizontalPos, HORIZONTAL_START);
            setItemIntProperty(item, settingsExt.Contrast, -1000, 1000, CONTRAST);
            setItemIntProperty(item, settingsExt.Brightness, -1000, 1000, BRIGHTNESS);
        }

        private void setupDevice()
        {
            switch (settingsExt.Source)
            {
                case ScanSource.GLASS:
                    setDeviceIntProperty(2, PAPER_SOURCE);
                    break;
                case ScanSource.FEEDER:
                    setDeviceIntProperty(1, PAPER_SOURCE);
                    break;
                case ScanSource.DUPLEX:
                    setDeviceIntProperty(4, PAPER_SOURCE);
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

        public ScannedImage GetImage()
        {
            CommonDialogClass WIACommonDialog = new CommonDialogClass();
            Image output;

            Items items = device.Items;
            if (settingsExt == null)
            {
                try
                {
                    items = WIACommonDialog.ShowSelectItems(device, WiaImageIntent.UnspecifiedIntent, WiaImageBias.MaximizeQuality, true, true, true);
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    if ((uint)e.ErrorCode == UI_CANCELED)
                        return null;
                }
            }
            else
            {
                setupDevice();
                setupItem(items[1]);
            }

            try
            {
                ImageFile file = (ImageFile)WIACommonDialog.ShowTransfer(items[1], "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}", false);
                if (file == null)
                {
                    // User cancelled
                    return null;
                }

                using (System.IO.MemoryStream stream = new System.IO.MemoryStream((byte[])file.FileData.get_BinaryData()))
                using (output = Image.FromStream(stream))
                {

                    double koef = 1;

                    switch (settingsExt.AfterScanScale)
                    {
                        case ScanScale.ONETOONE:
                            koef = 1;
                            break;
                        case ScanScale.ONETOTWO:
                            koef = 2;
                            break;
                        case ScanScale.ONETOFOUR:
                            koef = 4;
                            break;
                        case ScanScale.ONETOEIGHT:
                            koef = 8;
                            break;
                    }

                    double realWidth = output.Width / koef;
                    double realHeight = output.Height / koef;

                    double horizontalRes = output.HorizontalResolution / koef;
                    double verticalRes = output.VerticalResolution / koef;

                    using (Bitmap result = new Bitmap((int)realWidth, (int)realHeight))
                    using (Graphics g = Graphics.FromImage((Image)result))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(output, 0, 0, (int)realWidth, (int)realHeight);

                        result.SetResolution((float)horizontalRes, (float)verticalRes);

                        var bitDepth = settingsExt != null ? settingsExt.BitDepth : ScanBitDepth.C24BIT;
                        var imageFormat = settings.MaxQuality ? ImageFormat.Png : ImageFormat.Jpeg;
                        return new ScannedImage(result, bitDepth, imageFormat);
                    }
                }
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                if ((uint)e.ErrorCode == ERROR_OUT_OF_PAPER)
                {
                    return null;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("General scanning error!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return null;
                }
            }
        }
    }
}
