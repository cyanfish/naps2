using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Logging;
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

        public static void Scan(ScanProfile settings, ScanDevice device, IWin32Window pForm, IFormFactory formFactory, ScannedImageSource.Concrete source)
        {
            var tw = new Twain();
            var windowHandle = (Invoker.Current as Form)?.Handle ?? pForm.Handle;
            if (!tw.Init(windowHandle))
            {
                throw new DeviceNotFoundException();
            }
            if (!tw.SelectByName(device.ID))
            {
                throw new DeviceNotFoundException();
            }
            var form = Invoker.Current.InvokeGet(formFactory.Create<FTwainGui>);
            var mf = new TwainMessageFilter(settings, tw, form);
            Invoker.Current.Invoke(() => form.ShowDialog(pForm));
            foreach (var b in mf.Bitmaps)
            {
                source.Put(b);
            }
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

            public List<ScannedImage> Bitmaps { get; }

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
