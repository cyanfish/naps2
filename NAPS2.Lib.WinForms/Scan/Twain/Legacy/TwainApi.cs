using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using NAPS2.Images.Gdi;
using NAPS2.Platform.Windows;
using NAPS2.Scan.Exceptions;
using NAPS2.WinForms;

namespace NAPS2.Scan.Twain.Legacy;

internal static class TwainApi
{
    public static ScanDevice? SelectDeviceUI()
    {
        var tw = new Twain();
        if (!tw.Init(Application.OpenForms[0]!.Handle))
        {
            throw new NoDevicesFoundException();
        }
        if (!tw.Select())
        {
            return null;
        }
        string? name = tw.GetCurrentName();
        if (name == null)
        {
            return null;
        }
        return new ScanDevice(Driver.Twain, name, name);
    }

    public static List<ScanDevice> GetDeviceList(ScanOptions options)
    {
        var tw = new Twain();
        if (!tw.Init(options.DialogParent))
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
            string? name = tw.GetCurrentName();
            if (name != null)
            {
                result.Add(new ScanDevice(Driver.Twain, name, name));
            }
        } while (tw.GetNext());
        return result;
    }

    public static void Scan(ScanningContext scanningContext, ScanOptions options, Action<IMemoryImage> produceImage)
    {
        var tw = new Twain();
        if (!tw.Init(options.DialogParent))
        {
            throw new DeviceNotFoundException();
        }
        if (!tw.SelectByName(options.Device!.ID))
        {
            throw new DeviceNotFoundException();
        }
        var form = new FTwainGui();
        var mf = new TwainMessageFilter(scanningContext, options, tw, form);
        form.ShowDialog(new Win32Window(options.DialogParent));
        foreach (var b in mf.Bitmaps)
        {
            produceImage(b);
        }
    }

    private class TwainMessageFilter : IMessageFilter
    {
        private readonly ScanningContext _scanningContext;
        private readonly ScanOptions _settings;
        private readonly Twain _tw;
        private readonly FTwainGui _form;

        private bool _activated;
        private bool _msgfilter;

        public TwainMessageFilter(ScanningContext scanningContext, ScanOptions settings, Twain tw, FTwainGui form)
        {
            _scanningContext = scanningContext;
            _settings = settings;
            _tw = tw;
            _form = form;
            Bitmaps = [];
            form.Activated += FTwainGui_Activated;
        }

        public List<IMemoryImage> Bitmaps { get; }

        public bool PreFilterMessage(ref Message m)
        {
            TwainCommand cmd = _tw.PassMessage(ref m);
            if (cmd == TwainCommand.Not)
                return false;

            switch (cmd)
            {
                case TwainCommand.CloseRequest:
                {
                    EndingScan();
                    _tw.CloseSrc();
                    _form.Close();
                    break;
                }
                case TwainCommand.CloseOk:
                {
                    EndingScan();
                    _tw.CloseSrc();
                    break;
                }
                case TwainCommand.DeviceEvent:
                {
                    break;
                }
                case TwainCommand.TransferReady:
                {
                    ArrayList pics = _tw.TransferPictures();
                    EndingScan();
                    _tw.CloseSrc();
                    foreach (IntPtr img in pics)
                    {
                        int bitcount = 0;

                        Bitmap bmp = DibUtils.BitmapFromDib(img, out bitcount);
                        Bitmaps.Add(new GdiImage(_scanningContext.ImageContext, bmp));
                    }
                    _form.Close();
                    break;
                }
            }

            return true;
        }
        private void EndingScan()
        {
            if (_msgfilter)
            {
                Application.RemoveMessageFilter(this);
                _msgfilter = false;
                _form.Enabled = true;
                _form.Activate();
            }
        }

        private void FTwainGui_Activated(object? sender, EventArgs e)
        {
            if (_activated)
                return;
            _activated = true;
            if (!_msgfilter)
            {
                _form.Enabled = false;
                _msgfilter = true;
                Application.AddMessageFilter(this);
            }
            try
            {
                if (!_tw.Acquire())
                {
                    EndingScan();
                    _form.Close();
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException("An error occurred while interacting with TWAIN.", ex);
                EndingScan();
                _form.Close();
            }
        }
    }
}