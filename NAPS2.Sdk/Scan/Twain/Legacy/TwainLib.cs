using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NAPS2.Scan.Twain.Legacy;

internal enum TwainCommand
{
    Not = -1,
    Null = 0,
    TransferReady = 1,
    CloseRequest = 2,
    CloseOk = 3,
    DeviceEvent = 4
}

internal class Twain
{
    private const short COUNTRY_USA = 1;
    private const short LANGUAGE_USA = 13;
    private readonly TwIdentity _appid;
    private readonly TwIdentity _srcds;
    private TwEvent _evtmsg;
    private IntPtr _hwnd;
    private WINMSG _winmsg;

    public Twain()
    {
        _appid = new TwIdentity
        {
            Id = IntPtr.Zero,
            Version =
            {
                MajorNum = 1,
                MinorNum = 1,
                Language = LANGUAGE_USA,
                Country = COUNTRY_USA,
                Info = "Hack 1"
            },
            ProtocolMajor = TwProtocol.MAJOR,
            ProtocolMinor = TwProtocol.MINOR,
            SupportedGroups = (int)(TwDG.Image | TwDG.Control),
            Manufacturer = "NETMaster",
            ProductFamily = "Freeware",
            ProductName = "Hack"
        };

        _srcds = new TwIdentity { Id = IntPtr.Zero };

        _evtmsg.EventPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_winmsg));
    }

    public static int ScreenBitDepth
    {
        get
        {
            IntPtr screenDC = CreateDC("DISPLAY", null, null, IntPtr.Zero);
            int bitDepth = GetDeviceCaps(screenDC, 12);
            bitDepth *= GetDeviceCaps(screenDC, 14);
            DeleteDC(screenDC);
            return bitDepth;
        }
    }

    ~Twain()
    {
        Marshal.FreeHGlobal(_evtmsg.EventPtr);
    }

    public bool Init(IntPtr hwndp)
    {
        Finish();
        TwReturnCode returnCode = DSMparent(_appid, IntPtr.Zero, TwDG.Control, TwData.Parent, TwMessageCode.OpenDSM, ref hwndp);
        if (returnCode == TwReturnCode.Success)
        {
            returnCode = DSMident(_appid, IntPtr.Zero, TwDG.Control, TwData.Identity, TwMessageCode.GetFirst, _srcds);
            if (returnCode == TwReturnCode.Success)
            {
                _hwnd = hwndp;
                return true;
            }
            else
            {
                returnCode = DSMparent(_appid, IntPtr.Zero, TwDG.Control, TwData.Parent, TwMessageCode.CloseDSM, ref hwndp);
                return false;
            }
        }
        return false;
    }

    public bool GetFirst()
    {
        TwReturnCode returnCode;
        CloseSrc();
        if (_appid.Id == IntPtr.Zero)
        {
            Init(_hwnd);
            if (_appid.Id == IntPtr.Zero)
                return false;
        }
        returnCode = DSMident(_appid, IntPtr.Zero, TwDG.Control, TwData.Identity, TwMessageCode.GetFirst, _srcds);
        return returnCode == TwReturnCode.Success;
    }

    public bool GetNext()
    {
        TwReturnCode returnCode = DSMident(_appid, IntPtr.Zero, TwDG.Control, TwData.Identity, TwMessageCode.GetNext, _srcds);
        return returnCode == TwReturnCode.Success;
    }

    public bool Select()
    {
        TwReturnCode returnCode;
        CloseSrc();
        if (_appid.Id == IntPtr.Zero)
        {
            Init(_hwnd);
            if (_appid.Id == IntPtr.Zero)
                return false;
        }
        returnCode = DSMident(_appid, IntPtr.Zero, TwDG.Control, TwData.Identity, TwMessageCode.UserSelect, _srcds);
        return returnCode == TwReturnCode.Success;
    }

    public bool SelectByName(string name)
    {
        if (_srcds.ProductName == name)
        {
            return true;
        }
        var rc = TwReturnCode.Success;
        while (rc == TwReturnCode.Success)
        {
            rc = DSMident(_appid, IntPtr.Zero, TwDG.Control, TwData.Identity, TwMessageCode.GetNext, _srcds);
            if (_srcds.ProductName == name)
            {
                return true;
            }
        }
        return false;
    }

    public string GetCurrentName() => _srcds.ProductName;

    public bool Acquire()
    {
        TwReturnCode returnCode;
        CloseSrc();
        if (_appid.Id == IntPtr.Zero)
        {
            Init(_hwnd);
            if (_appid.Id == IntPtr.Zero)
                throw new InvalidOperationException("Init call falied");
        }
        returnCode = DSMident(_appid, IntPtr.Zero, TwDG.Control, TwData.Identity, TwMessageCode.OpenDS, _srcds);
        if (returnCode != TwReturnCode.Success)
            throw new InvalidOperationException("DSMident call falied");

        var guif = new TwUserInterface
        {
            ShowUI = 1,
            ModalUI = 1,
            ParentHand = _hwnd
        };
        returnCode = DSuserif(_appid, _srcds, TwDG.Control, TwData.UserInterface, TwMessageCode.EnableDS, guif);
        if (returnCode != TwReturnCode.Success)
        {
            CloseSrc();
            if (returnCode == TwReturnCode.Cancel)
            {
                return false;
            }
            throw new InvalidOperationException("DSuserif call falied");
        }
        return true;
    }

    public ArrayList TransferPictures()
    {
        var pics = new ArrayList();
        if (_srcds.Id == IntPtr.Zero)
            return pics;

        TwReturnCode returnCode;
        IntPtr hbitmap = IntPtr.Zero;
        var pxfr = new TwPendingXfers();

        do
        {
            pxfr.Count = 0;
            hbitmap = IntPtr.Zero;

            var iinf = new TwImageInfo();
            returnCode = DSiinf(_appid, _srcds, TwDG.Image, TwData.ImageInfo, TwMessageCode.Get, iinf);
            if (returnCode != TwReturnCode.Success)
            {
                CloseSrc();
                return pics;
            }

            returnCode = DSixfer(_appid, _srcds, TwDG.Image, TwData.ImageNativeXfer, TwMessageCode.Get, ref hbitmap);
            if (returnCode != TwReturnCode.XferDone)
            {
                CloseSrc();
                return pics;
            }

            returnCode = DSpxfer(_appid, _srcds, TwDG.Control, TwData.PendingXfers, TwMessageCode.EndXfer, pxfr);
            if (returnCode != TwReturnCode.Success)
            {
                CloseSrc();
                return pics;
            }

            pics.Add(hbitmap);
        }
        while (pxfr.Count != 0);

        returnCode = DSpxfer(_appid, _srcds, TwDG.Control, TwData.PendingXfers, TwMessageCode.Reset, pxfr);
        return pics;
    }

    public TwainCommand PassMessage(ref Message m)
    {
        if (_srcds.Id == IntPtr.Zero)
            return TwainCommand.Not;

        int pos = GetMessagePos();

        _winmsg.hwnd = m.HWnd;
        _winmsg.message = m.Msg;
        _winmsg.wParam = m.WParam;
        _winmsg.lParam = m.LParam;
        _winmsg.time = GetMessageTime();
        _winmsg.x = (short)pos;
        _winmsg.y = (short)(pos >> 16);

        Marshal.StructureToPtr(_winmsg, _evtmsg.EventPtr, false);
        _evtmsg.Message = 0;
        TwReturnCode returnCode = DSevent(_appid, _srcds, TwDG.Control, TwData.Event, TwMessageCode.ProcessEvent, ref _evtmsg);
        if (returnCode == TwReturnCode.NotDSEvent)
            return TwainCommand.Not;
        if (_evtmsg.Message == (short)TwMessageCode.XFerReady)
            return TwainCommand.TransferReady;
        if (_evtmsg.Message == (short)TwMessageCode.CloseDSReq)
            return TwainCommand.CloseRequest;
        if (_evtmsg.Message == (short)TwMessageCode.CloseDSOK)
            return TwainCommand.CloseOk;
        if (_evtmsg.Message == (short)TwMessageCode.DeviceEvent)
            return TwainCommand.DeviceEvent;

        return TwainCommand.Null;
    }

    public void CloseSrc()
    {
        TwReturnCode returnCode;
        if (_srcds.Id != IntPtr.Zero)
        {
            var guif = new TwUserInterface();
            returnCode = DSuserif(_appid, _srcds, TwDG.Control, TwData.UserInterface, TwMessageCode.DisableDS, guif);
            returnCode = DSMident(_appid, IntPtr.Zero, TwDG.Control, TwData.Identity, TwMessageCode.CloseDS, _srcds);
        }
    }

    public void Finish()
    {
        TwReturnCode returnCode;
        CloseSrc();
        if (_appid.Id != IntPtr.Zero)
            returnCode = DSMparent(_appid, IntPtr.Zero, TwDG.Control, TwData.Parent, TwMessageCode.CloseDSM, ref _hwnd);
        _appid.Id = IntPtr.Zero;
    }

    // ------ DSM entry point DAT_ variants:
    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwReturnCode DSMparent([In, Out] TwIdentity origin, IntPtr zeroptr, TwDG dg, TwData data, TwMessageCode messageCode, ref IntPtr refptr);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwReturnCode DSMident([In, Out] TwIdentity origin, IntPtr zeroptr, TwDG dg, TwData data, TwMessageCode messageCode, [In, Out] TwIdentity idds);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwReturnCode DSMstatus([In, Out] TwIdentity origin, IntPtr zeroptr, TwDG dg, TwData data, TwMessageCode messageCode, [In, Out] TwStatus dsmstat);

    // ------ DSM entry point DAT_ variants to DS:
    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwReturnCode DSuserif([In, Out] TwIdentity origin, [In, Out] TwIdentity dest, TwDG dg, TwData data, TwMessageCode messageCode, TwUserInterface guif);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwReturnCode DSevent([In, Out] TwIdentity origin, [In, Out] TwIdentity dest, TwDG dg, TwData data, TwMessageCode messageCode, ref TwEvent evt);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwReturnCode DSstatus([In, Out] TwIdentity origin, [In] TwIdentity dest, TwDG dg, TwData data, TwMessageCode messageCode, [In, Out] TwStatus dsmstat);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwReturnCode DScap([In, Out] TwIdentity origin, [In] TwIdentity dest, TwDG dg, TwData data, TwMessageCode messageCode, [In, Out] TwCapability capa);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwReturnCode DSiinf([In, Out] TwIdentity origin, [In] TwIdentity dest, TwDG dg, TwData data, TwMessageCode messageCode, [In, Out] TwImageInfo imginf);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwReturnCode DSixfer([In, Out] TwIdentity origin, [In] TwIdentity dest, TwDG dg, TwData data, TwMessageCode messageCode, ref IntPtr hbitmap);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwReturnCode DSpxfer([In, Out] TwIdentity origin, [In] TwIdentity dest, TwDG dg, TwData data, TwMessageCode messageCode, [In, Out] TwPendingXfers pxfr);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    internal static extern IntPtr GlobalAlloc(int flags, int size);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    internal static extern IntPtr GlobalLock(IntPtr handle);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    internal static extern bool GlobalUnlock(IntPtr handle);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    internal static extern IntPtr GlobalFree(IntPtr handle);

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int GetMessagePos();
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int GetMessageTime();

    [DllImport("gdi32.dll", ExactSpelling = true)]
    private static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr CreateDC(string szdriver, string szdevice, string szoutput, IntPtr devmode);

    [DllImport("gdi32.dll", ExactSpelling = true)]
    private static extern bool DeleteDC(IntPtr hdc);

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct WINMSG
    {
        public IntPtr hwnd;
        public int message;
        public IntPtr wParam;
        public IntPtr lParam;
        public int time;
        public int x;
        public int y;
    }
} // class Twain