/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.Scan.Twain
{
    internal partial class FTwainGui : Form, IMessageFilter
    {
        private readonly List<IScannedImage> bitmaps;
        private readonly ScanSettings settings;
        private bool activated;
        private bool msgfilter;
        private Twain tw;

        public FTwainGui(ScanSettings settings)
        {
            InitializeComponent();
            bitmaps = new List<IScannedImage>();
            this.settings = settings;
        }

        public List<IScannedImage> Bitmaps
        {
            get { return bitmaps; }
        }

        public Twain TwainIface
        {
            set { tw = value; }
        }

        bool IMessageFilter.PreFilterMessage(ref Message m)
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
                        Close();
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
                                bitmaps.Add(new ScannedImage(bmp, bitcount == 1 ? ScanBitDepth.BlackWhite : ScanBitDepth.C24Bit, settings.MaxQuality ? ImageFormat.Png : ImageFormat.Jpeg));
                            }
                        }
                        Close();
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
                Enabled = true;
                Activate();
            }
        }

        private void FTwainGui_Activated(object sender, EventArgs e)
        {
            if (activated)
                return;
            activated = true;
            if (!msgfilter)
            {
                Enabled = false;
                msgfilter = true;
                Application.AddMessageFilter(this);
            }
            tw.Acquire();
        }
    }
}
