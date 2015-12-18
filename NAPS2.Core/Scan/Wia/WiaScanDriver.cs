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
            using (var eventLoop = new WiaBackgroundEventLoop(ScanProfile, ScanDevice))
            {
                bool supportsFeeder = eventLoop.GetSync(wia => WiaApi.DeviceSupportsFeeder(wia.Device));
                if (ScanProfile.PaperSource != ScanSource.Glass && !supportsFeeder)
                {
                    throw new NoFeederSupportException();
                }
                int pageNumber = 1;
                while (true)
                {
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
                    if (ScanProfile.PaperSource == ScanSource.Glass)
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
                // TODO: Use the NoUI flag uniformly
                var transfer = ScanParams.NoUI ? new ConsoleWiaTransfer() : wiaTransfer;
                using (var stream = transfer.Transfer(pageNumber, eventLoop, WiaApi.Formats.BMP))
                {
                    if (stream == null)
                    {
                        // User cancelled
                        return null;
                    }
                    using (Image output = Image.FromStream(stream))
                    {
                        double scaleFactor = 1;
                        if (!ScanProfile.UseNativeUI)
                        {
                            scaleFactor = ScanProfile.AfterScanScale.ToIntScaleFactor();
                        }

                        using (var result = ImageScaleHelper.ScaleImage(output, scaleFactor))
                        {
                            ScanBitDepth bitDepth = ScanProfile.UseNativeUI ? ScanBitDepth.C24Bit : ScanProfile.BitDepth;
                            return scannedImageFactory.Create(result, bitDepth, ScanProfile.MaxQuality);
                        }
                    }
                }
            }
            catch (COMException e)
            {
                if ((uint)e.ErrorCode == WiaApi.Errors.OUT_OF_PAPER)
                {
                    if (ScanProfile.PaperSource != ScanSource.Glass && pageNumber == 1)
                    {
                        throw new NoPagesException();
                    }
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
