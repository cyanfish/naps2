/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2014  Ben Olden-Cooligan

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
using System.Windows.Forms;
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
            return WiaApi.SelectDeviceUI();
        }

        protected override IEnumerable<IScannedImage> ScanInternal()
        {
            using (var eventLoop = new WiaBackgroundEventLoop(ScanSettings, ScanDevice, scannedImageFactory))
            {
                bool supportsFeeder = false;
                eventLoop.Do(() =>
                {
                    supportsFeeder = eventLoop.Api.SupportsFeeder;
                });
                eventLoop.Sync();
                if (ScanSettings.PaperSource != ScanSource.Glass && !supportsFeeder)
                {
                    throw new NoFeederSupportException();
                }
                int pageNumber = 1;
                while (true)
                {
                    bool feederReady = false;
                    // TODO: Change eventloop api so that things that should only be accessed on the event loop thread
                    // TODO: (api, device, item) are accessible via an interface passed in as a single arg to the Do callback
                    eventLoop.Do(() =>
                    {
                        feederReady = eventLoop.Api.FeederReady;
                    });
                    eventLoop.Sync();
                    if (ScanSettings.PaperSource != ScanSource.Glass && !feederReady)
                    {
                        if (pageNumber == 1)
                        {
                            throw new NoPagesException();
                        }
                        break;
                    }
                    IScannedImage image = null;
                    try
                    {
                        // TODO: This method should be moved, since normally WiaApi calls should be done on event loop thread
                        image = eventLoop.Api.GetImage(wiaTransfer, eventLoop, pageNumber++);
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
    }
}
