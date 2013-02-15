using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using WIA;

namespace NAPS
{
    class CScannerAPI
    {
        private const int DEV_NAME = 7;
        private const int HORIZONTAL_FEED_SIZE = 3076;
        private const int HORIZONTAL_BED_SIZE = 3074;
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

        private Device device;

        private CScanSettings settings;

        public static string SelectDeviceUI()
        {
            CommonDialogClass WIACommonDialog = new CommonDialogClass();
            Device d;
            try
            {
                d = WIACommonDialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, true, false);
            }
            catch(System.Runtime.InteropServices.COMException e)
            {
                if ((uint)e.ErrorCode == NO_DEVICE_FOUND)
                {
                    throw new Exceptions.ENoScannerFound();
                }
                else
                {
                    throw e;
                }
            }
            return d.DeviceID;
        }

        public CScannerAPI(string DeviceID)
        {
            settings = new CScanSettings();
            DeviceManager manager = new DeviceManagerClass();
            foreach (DeviceInfo info in manager.DeviceInfos)
            {
                if (info.DeviceID == DeviceID)
                {
                    device = info.Connect();
                    return;
                }
            }
            throw new Exceptions.EScannerNotFound();
        }

        public CScannerAPI(CScanSettings settings)
        {
            this.settings = settings;
            DeviceManager manager = new DeviceManagerClass();
            foreach (DeviceInfo info in manager.DeviceInfos)
            {
                if (info.DeviceID == settings.DeviceID)
                {
                    device = info.Connect();
                    return;
                }
            }
            throw new Exceptions.EScannerNotFound();
        }

        private string getDeviceProperty(int propid)
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

        private void setDeviceIntProperty(int value,int propid)
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
            object objprop = value;
            foreach (WIA.Property property in item.Properties)
            {
                if (property.PropertyID == propid)
                {
                    property.set_Value(ref objprop);
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
            switch (settings.Depth)
            {
                case CScanSettings.BitDepth.GRAYSCALE:
                    setItemIntProperty(item, 2, DATA_TYPE);
                    break;
                case CScanSettings.BitDepth.C24BIT:
                    setItemIntProperty(item, 3, DATA_TYPE);
                    break;
                case CScanSettings.BitDepth.BLACKWHITE:
                    setItemIntProperty(item, 0, DATA_TYPE);
                    break;
            }

            switch (settings.Resolution)
            {
                case CScanSettings.DPI.DPI100:
                    setItemIntProperty(item, 100, VERTICAL_RESOLUTION);
                    setItemIntProperty(item, 100, HORIZONTAL_RESOLUTION);
                    resolution = 100;
                    break;
                case CScanSettings.DPI.DPI200:
                    setItemIntProperty(item, 200, VERTICAL_RESOLUTION);
                    setItemIntProperty(item, 200, HORIZONTAL_RESOLUTION);
                    resolution = 200;
                    break;
                case CScanSettings.DPI.DPI300:
                    setItemIntProperty(item, 300, VERTICAL_RESOLUTION);
                    setItemIntProperty(item, 300, HORIZONTAL_RESOLUTION);
                    resolution = 300;
                    break;
                case CScanSettings.DPI.DPI600:
                    setItemIntProperty(item, 600, VERTICAL_RESOLUTION);
                    setItemIntProperty(item, 600, HORIZONTAL_RESOLUTION);
                    resolution = 600;
                    break;
                case CScanSettings.DPI.DPI1200:
                    setItemIntProperty(item, 1200, VERTICAL_RESOLUTION);
                    setItemIntProperty(item, 1200, HORIZONTAL_RESOLUTION);
                    resolution = 120;
                    break;
            }

            Size pageSize = CPageSizes.TranslatePageSize(settings.PageSize);
            int pageWidth = pageSize.Width * resolution / 1000;
            int pageHeight = pageSize.Height * resolution / 1000;
            int horizontalSize = 0;
            if (settings.Source == CScanSettings.ScanSource.GLASS)
                horizontalSize = getDeviceIntProperty(HORIZONTAL_BED_SIZE);
            else
                horizontalSize = getDeviceIntProperty(HORIZONTAL_FEED_SIZE);
            int horizontalPos = 0;
            if (settings.PageAlign == CScanSettings.HorizontalAlign.CENTER)
                horizontalPos = (horizontalSize * resolution / 1000 - pageWidth) / 2;
            else if (settings.PageAlign == CScanSettings.HorizontalAlign.LEFT)
                horizontalPos = (horizontalSize * resolution / 1000 - pageWidth);
            setItemIntProperty(item, pageWidth, HORIZONTAL_EXTENT);
            setItemIntProperty(item, pageHeight, VERTICAL_EXTENT);
            setItemIntProperty(item, horizontalPos, HORIZONTAL_START);
            setItemIntProperty(item, settings.Contrast, CONTRAST);
            setItemIntProperty(item, settings.Brightnes, BRIGHTNESS);
        }

        private void setupDevice()
        {
            switch (settings.Source)
            {
                case CScanSettings.ScanSource.GLASS:
                    setDeviceIntProperty(2, PAPER_SOURCE);
                    break;
                case CScanSettings.ScanSource.FEEDER:
                    setDeviceIntProperty(1, PAPER_SOURCE);
                    break;
                case CScanSettings.ScanSource.DUPLEX:
                    setDeviceIntProperty(4, PAPER_SOURCE);
                    break;
            }
        }

        public Image GetImage()
        {
            CommonDialogClass WIACommonDialog = new CommonDialogClass();
            Image output;

            Items items = device.Items;
            if (settings.ShowScanUI)
            {
                items = WIACommonDialog.ShowSelectItems(device, WiaImageIntent.UnspecifiedIntent, WiaImageBias.MaximizeQuality, true, true, true);
            }
            else
            {
                setupDevice();
                setupItem(items[1]);
            }
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
            try
            {
                ImageFile file = (ImageFile)WIACommonDialog.ShowTransfer(items[1], "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}", false);
                System.IO.MemoryStream stream = new System.IO.MemoryStream((byte[])file.FileData.get_BinaryData());
                output = Image.FromStream(stream);

                double koef = 1;

                switch (settings.AfterScanScale)
                {
                    case CScanSettings.Scale.ONETOONE:
                        koef = 1;
                        break;
                    case CScanSettings.Scale.ONETOTWO:
                        koef = 2;
                        break;
                    case CScanSettings.Scale.ONETOFOUR:
                        koef = 4;
                        break;
                    case CScanSettings.Scale.ONETOEIGHT:
                        koef = 8;
                        break;
                }

                double realWidth = output.Width / koef;
                double realHeight = output.Height / koef;

                double horizontalRes = output.HorizontalResolution / koef;
                double verticalRes = output.VerticalResolution / koef;

                Bitmap result = new Bitmap((int)realWidth, (int)realHeight);
                Graphics g = Graphics.FromImage((Image)result);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(output, 0, 0, (int)realWidth, (int)realHeight);

                result.SetResolution((float)horizontalRes, (float)verticalRes);
                output = result;

            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                if ((uint)e.ErrorCode == ERROR_OUT_OF_PAPER)
                {
                    return null;
                }
                else
                {
                    throw e;
                }
            }


            return output;
        }

    }
}
