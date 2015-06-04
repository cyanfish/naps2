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
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.WinForms;
using NTwain;
using NTwain.Data;

namespace NAPS2.Scan.Twain
{
    // TODO: Create a ScanDriverBase class to remove the boilerplate from TwainScanDriver, then move this code there
    internal class TwainApi
    {
        private static readonly TWIdentity AppId = TWIdentity.CreateFromAssembly(DataGroups.Image | DataGroups.Control, Assembly.GetExecutingAssembly());

        private readonly IFormFactory formFactory;
        private readonly IScannedImageFactory scannedImageFactory;
        private readonly IWin32Window parent;
        private readonly ExtendedScanSettings settings;
        private readonly ScanDevice device;
        private readonly List<IScannedImage> images = new List<IScannedImage>();
        private FTwainGui twainForm;

        static TwainApi()
        {
            NTwain.PlatformInfo.Current.PreferNewDSM = false;
        }

        public TwainApi(ExtendedScanSettings settings, ScanDevice device, IWin32Window pForm, IFormFactory formFactory, IScannedImageFactory scannedImageFactory)
        {
            parent = pForm;
            this.formFactory = formFactory;
            this.scannedImageFactory = scannedImageFactory;
            this.settings = settings;
            this.device = device;
        }

        public static string SelectDeviceUI()
        {
            var session = new TwainSession(AppId);
            session.Open();
            try
            {
                var ds = session.ShowSourceSelector();
                if (ds == null)
                {
                    return null;
                }
                return ds.Name;
            }
            finally
            {
                session.Close();
            }
        }

        public List<IScannedImage> Scan()
        {
            var session = new TwainSession(AppId);
            session.TransferReady += TransferReady;
            session.DataTransferred += DataTransferred;
            session.TransferError += TransferError;
            session.SourceDisabled += SourceDisabled;
            session.Open();
            var ds = session.FirstOrDefault(x => x.Name == device.Name);
            try
            {
                if (ds == null)
                {
                    throw new DeviceNotFoundException();
                }
                ds.Open();
                twainForm = formFactory.Create<FTwainGui>();
                twainForm.DataSource = ds;
                twainForm.ShowDialog(parent);
                return images;
            }
            finally
            {
                if (ds != null)
                {
                    ds.Close();
                }
                session.Close();
            }
        }

        private void SourceDisabled(object sender, EventArgs eventArgs)
        {
            twainForm.Invoke((MethodInvoker)twainForm.Close);
        }

        private void DataTransferred(object sender, DataTransferredEventArgs eventArgs)
        {
            int bitcount;
            using (Bitmap bmp = DibUtils.BitmapFromDib(eventArgs.NativeData, out bitcount))
            {
                images.Add(scannedImageFactory.Create(bmp, bitcount == 1 ? ScanBitDepth.BlackWhite : ScanBitDepth.C24Bit, settings.MaxQuality));
            }
        }

        private void TransferError(object sender, TransferErrorEventArgs eventArgs)
        {
            Log.ErrorException("An error occurred while interacting with TWAIN.", eventArgs.Exception);
            twainForm.Close();
        }

        private void TransferReady(object sender, TransferReadyEventArgs eventArgs)
        {
        }
    }
}
