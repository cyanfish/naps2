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
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.WinForms;
using NTwain;
using NTwain.Data;

namespace NAPS2.Scan.Twain
{
    public class TwainScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "twain";
        private static readonly TWIdentity TwainAppId = TWIdentity.CreateFromAssembly(DataGroups.Image | DataGroups.Control, Assembly.GetEntryAssembly());

        private readonly IFormFactory formFactory;
        private readonly IScannedImageFactory scannedImageFactory;

        static TwainScanDriver()
        {
            NTwain.PlatformInfo.Current.PreferNewDSM = false;
        }

        public TwainScanDriver(IFormFactory formFactory, IScannedImageFactory scannedImageFactory)
        {
            this.formFactory = formFactory;
            this.scannedImageFactory = scannedImageFactory;
        }

        public override string DriverName
        {
            get { return DRIVER_NAME; }
        }

        protected override ScanDevice PromptForDeviceInternal()
        {
            var session = new TwainSession(TwainAppId);
            session.Open();
            try
            {
                var ds = session.ShowSourceSelector();
                if (ds == null)
                {
                    return null;
                }
                string deviceId = ds.Name;
                string deviceName = ds.Name;
                return new ScanDevice(deviceId, deviceName);
            }
            finally
            {
                session.Close();
            }
        }

        protected override IEnumerable<IScannedImage> ScanInternal()
        {
            var session = new TwainSession(TwainAppId);
            var twainForm = formFactory.Create<FTwainGui>();
            var images = new List<IScannedImage>();
            Exception error = null;
            bool cancel = false;
            DataSource ds = null;

            session.TransferReady += (sender, eventArgs) =>
            {
                if (cancel)
                {
                    eventArgs.CancelAll = true;
                }
            };
            session.DataTransferred += (sender, eventArgs) =>
            {
                using (var output = Image.FromStream(eventArgs.GetNativeImageStream()))
                {
                    double scaleFactor = 1;
                    if (!ScanProfile.UseNativeUI)
                    {
                        scaleFactor = ScanProfile.AfterScanScale.ToIntScaleFactor();
                    }

                    using (var result = ImageScaleHelper.ScaleImage(output, scaleFactor))
                    {
                        var bitDepth = output.PixelFormat == PixelFormat.Format1bppIndexed
                            ? ScanBitDepth.BlackWhite
                            : ScanBitDepth.C24Bit;
                        var img = scannedImageFactory.Create(result, bitDepth, ScanProfile.MaxQuality);
                        if (ScanParams.DetectPatchCodes)
                        {
                            foreach (var patchCodeInfo in eventArgs.GetExtImageInfo(ExtendedImageInfo.PatchCode))
                            {
                                if (patchCodeInfo.ReturnCode == ReturnCode.Success)
                                {
                                    img.PatchCode = GetPatchCode(patchCodeInfo);
                                }
                            }
                        }
                        images.Add(img);
                    }
                }
            };
            session.TransferError += (sender, eventArgs) =>
            {
                error = eventArgs.Exception;
                cancel = true;
                twainForm.Close();
            };
            session.SourceDisabled += (sender, eventArgs) => twainForm.Close();

            twainForm.Shown += (sender, eventArgs) =>
            {
                try
                {
                    ReturnCode rc = session.Open(new WindowsFormsMessageLoopHook(DialogParent.Handle));
                    if (rc != ReturnCode.Success)
                    {
                        twainForm.Close();
                        return;
                    }
                    ds = session.FirstOrDefault(x => x.Name == ScanDevice.ID);
                    if (ds == null)
                    {
                        throw new DeviceNotFoundException();
                    }
                    rc = ds.Open();
                    if (rc != ReturnCode.Success)
                    {
                        twainForm.Close();
                        return;
                    }
                    ConfigureDS(ds);
                    var ui = ScanProfile.UseNativeUI ? SourceEnableMode.ShowUI : SourceEnableMode.NoUI;
                    rc = ds.Enable(ui, true, twainForm.Handle);
                    if (rc != ReturnCode.Success)
                    {
                        twainForm.Close();
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                    twainForm.Close();
                }
            };

            twainForm.ShowDialog(DialogParent);

            if (ds != null && session.IsSourceOpen)
            {
                ds.Close();
            }
            if (session.IsDsmOpen)
            {
                session.Close();
            }

            if (error != null)
            {
                if (error is ScanDriverException)
                {
                    throw error;
                }
                throw new ScanDriverUnknownException(error);
            }

            return images;
        }

        private static PatchCode GetPatchCode(TWInfo patchCodeInfo)
        {
            switch ((NTwain.Data.PatchCode)patchCodeInfo.Item)
            {
                case NTwain.Data.PatchCode.Patch1:
                    return PatchCode.Patch1;
                case NTwain.Data.PatchCode.Patch2:
                    return PatchCode.Patch2;
                case NTwain.Data.PatchCode.Patch3:
                    return PatchCode.Patch3;
                case NTwain.Data.PatchCode.Patch4:
                    return PatchCode.Patch4;
                case NTwain.Data.PatchCode.Patch6:
                    return PatchCode.Patch6;
                case NTwain.Data.PatchCode.PatchT:
                    return PatchCode.PatchT;
                default:
                    throw new ArgumentException();
            }
        }

        private void ConfigureDS(DataSource ds)
        {
            if (ScanProfile.UseNativeUI)
            {
                return;
            }

            // Paper Source
            switch (ScanProfile.PaperSource)
            {
                case ScanSource.Glass:
                    ds.Capabilities.CapFeederEnabled.SetValue(BoolType.False);
                    ds.Capabilities.CapDuplexEnabled.SetValue(BoolType.False);
                    break;
                case ScanSource.Feeder:
                    ds.Capabilities.CapFeederEnabled.SetValue(BoolType.True);
                    ds.Capabilities.CapDuplexEnabled.SetValue(BoolType.False);
                    break;
                case ScanSource.Duplex:
                    ds.Capabilities.CapFeederEnabled.SetValue(BoolType.True);
                    ds.Capabilities.CapDuplexEnabled.SetValue(BoolType.True);
                    break;
            }

            // Bit Depth
            switch (ScanProfile.BitDepth)
            {
                case ScanBitDepth.C24Bit:
                    ds.Capabilities.ICapPixelType.SetValue(PixelType.RGB);
                    break;
                case ScanBitDepth.Grayscale:
                    ds.Capabilities.ICapPixelType.SetValue(PixelType.Gray);
                    break;
                case ScanBitDepth.BlackWhite:
                    ds.Capabilities.ICapPixelType.SetValue(PixelType.BlackWhite);
                    break;
            }

            // Page Size, Horizontal Align
            PageDimensions pageDimensions = ScanProfile.PageSize.PageDimensions() ?? ScanProfile.CustomPageSize;
            if (pageDimensions == null)
            {
                throw new InvalidOperationException("No page size specified");
            }
            float pageWidth = pageDimensions.WidthInThousandthsOfAnInch() / 1000.0f;
            float pageHeight = pageDimensions.HeightInThousandthsOfAnInch() / 1000.0f;
            var pageMaxWidthFixed = ds.Capabilities.ICapPhysicalWidth.GetCurrent();
            float pageMaxWidth = pageMaxWidthFixed.Whole + (pageMaxWidthFixed.Fraction / (float)ushort.MaxValue);

            float horizontalOffset = 0.0f;
            if (ScanProfile.PageAlign == ScanHorizontalAlign.Center)
                horizontalOffset = (pageMaxWidth - pageWidth) / 2;
            else if (ScanProfile.PageAlign == ScanHorizontalAlign.Left)
                horizontalOffset = (pageMaxWidth - pageWidth);

            ds.Capabilities.ICapUnits.SetValue(Unit.Inches);
            TWImageLayout imageLayout;
            ds.DGImage.ImageLayout.Get(out imageLayout);
            imageLayout.Frame = new TWFrame
            {
                Left = horizontalOffset,
                Right = horizontalOffset + pageWidth,
                Top = 0,
                Bottom = pageHeight
            };
            ds.DGImage.ImageLayout.Set(imageLayout);

            // Brightness, Contrast
            // Conveniently, the range of values used in settings (-1000 to +1000) is the same range TWAIN supports
            ds.Capabilities.ICapBrightness.SetValue(ScanProfile.Brightness);
            ds.Capabilities.ICapContrast.SetValue(ScanProfile.Contrast);

            // Resolution
            int dpi = ScanProfile.Resolution.ToIntDpi();
            ds.Capabilities.ICapXResolution.SetValue(dpi);
            ds.Capabilities.ICapYResolution.SetValue(dpi);

            // Patch codes
            if (ScanParams.DetectPatchCodes)
            {
                ds.Capabilities.ICapPatchCodeDetectionEnabled.SetValue(BoolType.True);
            }
        }
    }
}
