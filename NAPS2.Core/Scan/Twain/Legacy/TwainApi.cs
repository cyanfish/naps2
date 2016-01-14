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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.Scan.Twain.Legacy
{
    internal static class TwainApi
    {
        public static ScanDevice SelectDeviceUI()
        {
            var tw = new Twain();
            if (!tw.Init(Application.OpenForms[0].Handle))
            {
                throw new NoDevicesFoundException();
            }
            if (!tw.Select())
            {
                return null;
            }
            string name = tw.GetCurrentName();
            if (name == null)
            {
                return null;
            }
            return new ScanDevice(name, name);
        }

        public static List<ScanDevice> GetDeviceList()
        {
            var tw = new Twain();
            if (!tw.Init(Application.OpenForms[0].Handle))
            {
                throw new NoDevicesFoundException();
            }
            var result = new List<ScanDevice>();
            if (!tw.GetFirst())
            {
                return result;
            }
            do
            {
                string name = tw.GetCurrentName();
                result.Add(new ScanDevice(name, name));
            } while (tw.GetNext());
            return result;
        }

        public static List<ScannedImage> Scan(ScanProfile settings, ScanDevice device, IWin32Window pForm, IFormFactory formFactory)
        {
            var tw = new Twain();
            if (!tw.Init(pForm.Handle))
            {
                throw new DeviceNotFoundException();
            }
            if (!tw.SelectByName(device.ID))
            {
                throw new DeviceNotFoundException();
            }
            var form = formFactory.Create<FTwainGui>();
            var mf = new TwainMessageFilter(settings, tw, form);
            form.ShowDialog(pForm);
            return mf.Bitmaps;
        }

        private class TwainMessageFilter : IMessageFilter
        {
            private readonly ScanProfile settings;
            private readonly Twain tw;
            private readonly FTwainGui form;

            private bool activated;
            private bool msgfilter;

            public TwainMessageFilter(ScanProfile settings, Twain tw, FTwainGui form)
            {
                this.settings = settings;
                this.tw = tw;
                this.form = form;
                Bitmaps = new List<ScannedImage>();
                form.Activated += FTwainGui_Activated;
            }

            public List<ScannedImage> Bitmaps { get; private set; }

            public bool PreFilterMessage(ref Message m)
            {
                TwainCommand cmd = tw.PassMessage(ref m);
                if (cmd == TwainCommand.Not)
                    return false;

                switch (cmd)
                {
                    case TwainCommand.CloseRequest:
                        {
                            EndingScan();
                            tw.CloseSrc();
                            form.Close();
                            break;
                        }
                    case TwainCommand.CloseOk:
                        {
                            EndingScan();
                            tw.CloseSrc();
                            break;
                        }
                    case TwainCommand.DeviceEvent:
                        {
                            break;
                        }
                    case TwainCommand.TransferReady:
                        {
                            ArrayList pics = tw.TransferPictures();
                            EndingScan();
                            tw.CloseSrc();
                            foreach (IntPtr img in pics)
                            {
                                int bitcount = 0;

                                using (Bitmap bmp = DibUtils.BitmapFromDib(img, out bitcount))
                                {
                                    Bitmaps.Add(new ScannedImage(bmp, bitcount == 1 ? ScanBitDepth.BlackWhite : ScanBitDepth.C24Bit, settings.MaxQuality, settings.Quality));
                                }
                            }
                            form.Close();
                            break;
                        }
                }

                return true;
            }
            private void EndingScan()
            {
                if (msgfilter)
                {
                    Application.RemoveMessageFilter(this);
                    msgfilter = false;
                    form.Enabled = true;
                    form.Activate();
                }
            }

            private void FTwainGui_Activated(object sender, EventArgs e)
            {
                if (activated)
                    return;
                activated = true;
                if (!msgfilter)
                {
                    form.Enabled = false;
                    msgfilter = true;
                    Application.AddMessageFilter(this);
                }
                try
                {
                    if (!tw.Acquire())
                    {
                        EndingScan();
                        form.Close();
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorException("An error occurred while interacting with TWAIN.", ex);
                    EndingScan();
                    form.Close();
                }
            }
        }
    }
}
