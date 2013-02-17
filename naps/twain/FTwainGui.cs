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
