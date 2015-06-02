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
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NAPS2.Scan.Twain
{
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
        // http://www.twain.org/docs/530fe0cb85f7511c510004e8/Twain-Spec-1-9-197.pdf
        // A bit of summary information for reference:
        // DSM = Data Source Manager, handles choosing and opening/closing data sources
        // DS = Data Source, provides image data (i.e. a scanner)
        // dsm* methods are called on the DSM, ds* are passed through to the DS
        // Since they use the same entry point, they are differentiated by IntPtr.Zero vs TwIdentity for the second arg.
        // Method variants are simply for different argument types, they all go to the same entry point.
        // Refer to the spec for specific messages that can be sent and the required arguments.

        private const short COUNTRY_USA = 1;
        private const short LANGUAGE_USA = 13;
        private readonly TwIdentity appid;
        private readonly TwIdentity srcds;
        private TwEvent evtmsg;
        private IntPtr hwnd;
        private WINMSG winmsg;

        public Twain()
        {
            appid = new TwIdentity
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

            srcds = new TwIdentity { Id = IntPtr.Zero };

            evtmsg.EventPtr = Marshal.AllocHGlobal(Marshal.SizeOf(winmsg));
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
            Marshal.FreeHGlobal(evtmsg.EventPtr);
        }

        /// <summary>
        /// Initializes the Data Source Manager (DSM).
        /// </summary>
        /// <param name="hwndp">A pointer to the parent window's handle.</param>
        /// <returns></returns>
        public bool InitDSM(IntPtr hwndp)
        {
            Finish();
            TwReturnCode returnCode = DSMparent(appid, IntPtr.Zero, TwDG.Control, TwData.Parent, TwMessageCode.OpenDSM, ref hwndp);
            if (returnCode == TwReturnCode.Success)
            {
                returnCode = DSMident(appid, IntPtr.Zero, TwDG.Control, TwData.Identity, TwMessageCode.GetFirst, srcds);
                if (returnCode == TwReturnCode.Success)
                {
                    hwnd = hwndp;
                    return true;
                }
                else
                {
                    returnCode = DSMparent(appid, IntPtr.Zero, TwDG.Control, TwData.Parent, TwMessageCode.CloseDSM, ref hwndp);
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Displays a modal UI for the user to choose a TWAIN device. The chosen device is stored in the srcds field (if successful).
        /// </summary>
        /// <returns></returns>
        public bool Select()
        {
            TwReturnCode returnCode;
            CloseDS();
            if (appid.Id == IntPtr.Zero)
            {
                InitDSM(hwnd);
                if (appid.Id == IntPtr.Zero)
                    return false;
            }
            returnCode = DSMident(appid, IntPtr.Zero, TwDG.Control, TwData.Identity, TwMessageCode.UserSelect, srcds);
            return returnCode == TwReturnCode.Success;
        }

        /// <summary>
        /// Chooses the TWAIN device with the given name. The chosen device is stored in the srcds field (if successful).
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <returns></returns>
        public bool SelectByName(string name)
        {
            if (srcds.ProductName == name)
            {
                return true;
            }
            var rc = TwReturnCode.Success;
            while (rc == TwReturnCode.Success)
            {
                rc = DSMident(appid, IntPtr.Zero, TwDG.Control, TwData.Identity, TwMessageCode.GetNext, srcds);
                if (srcds.ProductName == name)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the name of the most-recently chosen TWAIN device.
        /// </summary>
        /// <returns></returns>
        public string GetCurrentName()
        {
            return srcds.ProductName;
        }

        /// <summary>
        /// Start acquiring the image data. Runs asynchronously.
        /// </summary>
        /// <returns></returns>
        public bool Acquire()
        {
            TwReturnCode returnCode;

            CloseDS();
            if (appid.Id == IntPtr.Zero)
            {
                InitDSM(hwnd);
                if (appid.Id == IntPtr.Zero)
                    throw new InvalidOperationException("Init call falied");
            }

            // Tell the DSM to initialize the DS
            returnCode = DSMident(appid, IntPtr.Zero, TwDG.Control, TwData.Identity, TwMessageCode.OpenDS, srcds);
            if (returnCode != TwReturnCode.Success)
                throw new InvalidOperationException("DSMident call falied");

            // Display the user interface, which will begin acquiring the image when appropriate
            var guif = new TwUserInterface
            {
                ShowUI = 1,
                ModalUI = 1,
                ParentHand = hwnd
            };
            returnCode = DSuserif(appid, srcds, TwDG.Control, TwData.UserInterface, TwMessageCode.EnableDS, guif);

            if (returnCode != TwReturnCode.Success)
            {
                CloseDS();

                if (returnCode == TwReturnCode.Cancel)
                {
                    return false;
                }
                throw new InvalidOperationException("DSuserif call falied");
            }
            return true;
        }

        /// <summary>
        /// When the image data is ready to transfer, call this method to get an array of DIB pointers.
        /// </summary>
        /// <returns>An array of DIB pointers.</returns>
        public ArrayList TransferPictures()
        {
            var pics = new ArrayList();
            if (srcds.Id == IntPtr.Zero)
                return pics;

            TwReturnCode returnCode;
            IntPtr hbitmap = IntPtr.Zero;
            var pxfr = new TwPendingXfers();

            do
            {
                pxfr.Count = 0;
                hbitmap = IntPtr.Zero;

                var iinf = new TwImageInfo();
                returnCode = DSiinf(appid, srcds, TwDG.Image, TwData.ImageInfo, TwMessageCode.Get, iinf);
                if (returnCode != TwReturnCode.Success)
                {
                    CloseDS();
                    return pics;
                }

                returnCode = DSixfer(appid, srcds, TwDG.Image, TwData.ImageNativeXfer, TwMessageCode.Get, ref hbitmap);
                if (returnCode != TwReturnCode.XferDone)
                {
                    CloseDS();
                    return pics;
                }

                returnCode = DSpxfer(appid, srcds, TwDG.Control, TwData.PendingXfers, TwMessageCode.EndXfer, pxfr);
                if (returnCode != TwReturnCode.Success)
                {
                    CloseDS();
                    return pics;
                }

                pics.Add(hbitmap);
            }
            while (pxfr.Count != 0);

            returnCode = DSpxfer(appid, srcds, TwDG.Control, TwData.PendingXfers, TwMessageCode.Reset, pxfr);
            return pics;
        }

        public TwainCommand PassMessage(ref Message m)
        {
            if (srcds.Id == IntPtr.Zero)
                return TwainCommand.Not;

            int pos = GetMessagePos();

            winmsg.hwnd = m.HWnd;
            winmsg.message = m.Msg;
            winmsg.wParam = m.WParam;
            winmsg.lParam = m.LParam;
            winmsg.time = GetMessageTime();
            winmsg.x = (short)pos;
            winmsg.y = (short)(pos >> 16);

            Marshal.StructureToPtr(winmsg, evtmsg.EventPtr, false);
            evtmsg.Message = 0;
            TwReturnCode returnCode = DSevent(appid, srcds, TwDG.Control, TwData.Event, TwMessageCode.ProcessEvent, ref evtmsg);
            if (returnCode == TwReturnCode.NotDSEvent)
                return TwainCommand.Not;
            if (evtmsg.Message == (short)TwMessageCode.XFerReady)
                return TwainCommand.TransferReady;
            if (evtmsg.Message == (short)TwMessageCode.CloseDSReq)
                return TwainCommand.CloseRequest;
            if (evtmsg.Message == (short)TwMessageCode.CloseDSOK)
                return TwainCommand.CloseOk;
            if (evtmsg.Message == (short)TwMessageCode.DeviceEvent)
                return TwainCommand.DeviceEvent;

            return TwainCommand.Null;
        }

        /// <summary>
        /// Closes the current Data Source (DS).
        /// </summary>
        public void CloseDS()
        {
            TwReturnCode returnCode;
            if (srcds.Id != IntPtr.Zero)
            {
                var guif = new TwUserInterface();
                returnCode = DSuserif(appid, srcds, TwDG.Control, TwData.UserInterface, TwMessageCode.DisableDS, guif);
                returnCode = DSMident(appid, IntPtr.Zero, TwDG.Control, TwData.Identity, TwMessageCode.CloseDS, srcds);
            }
        }

        public void Finish()
        {
            TwReturnCode returnCode;
            CloseDS();
            if (appid.Id != IntPtr.Zero)
                returnCode = DSMparent(appid, IntPtr.Zero, TwDG.Control, TwData.Parent, TwMessageCode.CloseDSM, ref hwnd);
            appid.Id = IntPtr.Zero;
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
}
