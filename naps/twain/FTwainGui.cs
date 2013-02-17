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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace NAPS.twain
{
    public partial class FTwainGui : Form, IMessageFilter
    {
        private bool activated = false;
        private CScanSettings settings;

        public FTwainGui(CScanSettings settings)
        {
            InitializeComponent();
            bitmaps = new List<CScannedImage>();
            this.settings = settings;
        }

        private List<CScannedImage> bitmaps;

        public List<CScannedImage> Bitmaps
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
                        this.Close();
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
                        for (int i = 0; i < pics.Count; i++)
                        {
                            IntPtr img = (IntPtr)pics[i];
                            int bitcount = 0;

                            using (Bitmap bmp = CDIBUtils.BitmapFromDIB(img, out bitcount))
                            {
                                bitmaps.Add(new CScannedImage(bmp, bitcount == 1 ? CScanSettings.BitDepth.BLACKWHITE : CScanSettings.BitDepth.C24BIT, settings.HighQuality ? ImageFormat.Png : ImageFormat.Jpeg));
                            }
                        }
                        this.Close();
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
                this.Enabled = true;
                this.Activate();
            }
        }

        private bool msgfilter;
        private Twain tw;

        private void FTwainGui_Activated(object sender, EventArgs e)
        {
            if (activated)
                return;
            activated = true;
            if (!msgfilter)
            {
                this.Enabled = false;
                msgfilter = true;
                Application.AddMessageFilter(this);
            }
            tw.Acquire();
        }
    }
}
