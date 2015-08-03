/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
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
using System.Linq;
using System.Runtime.InteropServices;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;

namespace NAPS2.Scan.Wia
{
    public class WiaScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "wia";

        private readonly IScannedImageFactory scannedImageFactory;
        private readonly IWiaTransfer wiaTransfer;

        public WiaScanDriver(IScannedImageFactory scannedImageFactory, IWiaTransfer wiaTransfer)
        {
            this.scannedImageFactory = scannedImageFactory;
            this.wiaTransfer = wiaTransfer;
        }

        public override string DriverName
        {
            get { return DRIVER_NAME; }
        }

        protected override ScanDevice PromptForDeviceInternal()
        {
            return WiaApi.PromptForDevice();
        }

        protected override IEnumerable<IScannedImage> ScanInternal()
        {
            using (var eventLoop = new WiaBackgroundEventLoop(ScanSettings, ScanDevice))
            {
                bool supportsFeeder = eventLoop.GetSync(wia => WiaApi.DeviceSupportsFeeder(wia.Device));
                if (ScanSettings.PaperSource != ScanSource.Glass && !supportsFeeder)
                {
                    throw new NoFeederSupportException();
                }
                int pageNumber = 1;
                while (true)
                {
                    bool feederReady = eventLoop.GetSync(wia => WiaApi.DeviceFeederReady(wia.Device));
                    if (ScanSettings.PaperSource != ScanSource.Glass && !feederReady)
                    {
                        if (pageNumber == 1)
                        {
                            throw new NoPagesException();
                        }
                        break;
                    }
                    IScannedImage image;
                    try
                    {
                        image = TransferImage(eventLoop, pageNumber++);
                    }
                    catch (ScanDriverException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        throw new ScanDriverUnknownException(e);
                    }
                    if (image == null)
                    {
                        break;
                    }
                    yield return image;
                    if (ScanSettings.PaperSource == ScanSource.Glass)
                    {
                        break;
                    }
                }
            }
        }

        private IScannedImage TransferImage(WiaBackgroundEventLoop eventLoop, int pageNumber)
        {
            try
            {
                using (var stream = wiaTransfer.Transfer(pageNumber, eventLoop, WiaApi.Formats.BMP))
                {
                    if (stream == null)
                    {
                        // User cancelled
                        return null;
                    }
                    using (Image output = Image.FromStream(stream))
                    {
                        double scaleFactor = 1;
                        if (!ScanSettings.UseNativeUI)
                        {
                            scaleFactor = ScanSettings.AfterScanScale.ToIntScaleFactor();
                        }

                        using (var result = ImageScaleHelper.ScaleImage(output, scaleFactor))
                        {
                            ScanBitDepth bitDepth = ScanSettings.UseNativeUI ? ScanBitDepth.C24Bit : ScanSettings.BitDepth;
                            return scannedImageFactory.Create(result, bitDepth, ScanSettings.MaxQuality);
                        }
                    }
                }
            }
            catch (COMException e)
            {
                if ((uint)e.ErrorCode == WiaApi.Errors.OUT_OF_PAPER)
                {
                    return null;
                }
                else if ((uint)e.ErrorCode == WiaApi.Errors.OFFLINE)
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
