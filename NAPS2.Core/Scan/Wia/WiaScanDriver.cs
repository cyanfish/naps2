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
using NAPS2.Config;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.Scan.Wia
{
    public class WiaScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "wia";

        private readonly IWiaTransfer backgroundWiaTransfer;
        private readonly IWiaTransfer foregroundWiaTransfer;
        private readonly ThreadFactory threadFactory;
        private readonly IBlankDetector blankDetector;
        private readonly ThumbnailRenderer thumbnailRenderer;

        public WiaScanDriver(BackgroundWiaTransfer backgroundWiaTransfer, ForegroundWiaTransfer foregroundWiaTransfer, ThreadFactory threadFactory, IBlankDetector blankDetector, ThumbnailRenderer thumbnailRenderer)
        {
            this.backgroundWiaTransfer = backgroundWiaTransfer;
            this.foregroundWiaTransfer = foregroundWiaTransfer;
            this.threadFactory = threadFactory;
            this.blankDetector = blankDetector;
            this.thumbnailRenderer = thumbnailRenderer;
        }

        public override string DriverName
        {
            get { return DRIVER_NAME; }
        }

        protected override ScanDevice PromptForDeviceInternal()
        {
            return WiaApi.PromptForScanDevice();
        }

        protected override List<ScanDevice> GetDeviceListInternal()
        {
            return WiaApi.GetScanDeviceList();
        }

        protected override IEnumerable<ScannedImage> ScanInternal()
        {
            using (var eventLoop = new WiaBackgroundEventLoop(ScanProfile, ScanDevice, threadFactory))
            {
                bool supportsFeeder = eventLoop.GetSync(wia => WiaApi.DeviceSupportsFeeder(wia.Device));
                if (ScanProfile.PaperSource != ScanSource.Glass && !supportsFeeder)
                {
                    throw new NoFeederSupportException();
                }
                bool supportsDuplex = eventLoop.GetSync(wia => WiaApi.DeviceSupportsDuplex(wia.Device));
                if (ScanProfile.PaperSource == ScanSource.Duplex && !supportsDuplex)
                {
                    throw new NoDuplexSupportException();
                }
                int pageNumber = 1;
                bool cancel;
                do
                {
                    ScannedImage image;
                    try
                    {
                        image = TransferImage(eventLoop, pageNumber++, out cancel);
                    }
                    catch (ScanDriverException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        throw new ScanDriverUnknownException(e);
                    }
                    if (image != null)
                    {
                        yield return image;
                    }
                } while (!cancel && ScanProfile.PaperSource != ScanSource.Glass);
            }
        }

        private ScannedImage TransferImage(WiaBackgroundEventLoop eventLoop, int pageNumber, out bool cancel)
        {
            try
            {
                var transfer = ScanParams.NoUI ? backgroundWiaTransfer : foregroundWiaTransfer;
                using (var stream = transfer.Transfer(pageNumber, eventLoop, WiaApi.Formats.BMP))
                {
                    if (stream == null)
                    {
                        cancel = true;
                        return null;
                    }
                    cancel = false;
                    using (Image output = Image.FromStream(stream))
                    {
                        using (var result = ScannedImageHelper.PostProcessStep1(output, ScanProfile))
                        {
                            if (blankDetector.ExcludePage(result, ScanProfile))
                            {
                                return null;
                            }
                            ScanBitDepth bitDepth = ScanProfile.UseNativeUI ? ScanBitDepth.C24Bit : ScanProfile.BitDepth;
                            var image = new ScannedImage(result, bitDepth, ScanProfile.MaxQuality, ScanProfile.Quality);
                            image.SetThumbnail(thumbnailRenderer.RenderThumbnail(result));
                            ScannedImageHelper.PostProcessStep2(image, result, ScanProfile, ScanParams, pageNumber);
                            return image;
                        }
                    }
                }
            }
            catch (NoPagesException)
            {
                if (ScanProfile.PaperSource != ScanSource.Glass && pageNumber == 1)
                {
                    // No pages were in the feeder, so show the user an error
                    throw new NoPagesException();
                }
                // At least one page was scanned but now the feeder is empty, so exit normally
                cancel = true;
                return null;
            }
            catch (ScanDriverException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ScanDriverUnknownException(e);
            }
        }
    }
}
