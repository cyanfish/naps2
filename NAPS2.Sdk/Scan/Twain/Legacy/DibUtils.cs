using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using NAPS2.Util;

namespace NAPS2.Scan.Twain.Legacy
{
    internal class DibUtils
    {
        [DllImport("gdi32.dll", ExactSpelling = true)]
        internal static extern int SetDIBitsToDevice(IntPtr hdc, int xdst, int ydst,
        int width, int height, int xsrc, int ysrc, int start, int lines,
        IntPtr bitsptr, IntPtr bmiptr, int color);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GlobalLock(IntPtr handle);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GlobalFree(IntPtr handle);

        //THIS METHOD SAVES THE CONTENTS OF THE DIB POINTER INTO A BITMAP OBJECT
        public static Bitmap BitmapFromDib(IntPtr pDib, out int bitdepth)
        {
            IntPtr dibhand = pDib;
            IntPtr bmpptr = GlobalLock(dibhand);
            IntPtr pixptr = GetPixelInfo(bmpptr);
            BitmapInfoHeader binfo = GetDibInfo(bmpptr);
            float resx = binfo.biXPelsPerMeter * 0.0254f;
            float resy = binfo.biYPelsPerMeter * 0.0254f;
            var scannedImage = new Bitmap(binfo.biWidth, binfo.biHeight);
            Graphics scannedImageGraphics = Graphics.FromImage(scannedImage);
            IntPtr hdc = scannedImageGraphics.GetHdc();
            SetDIBitsToDevice(hdc, 0, 0, binfo.biWidth, binfo.biHeight, 0, 0, 0, binfo.biHeight, pixptr, bmpptr, 0);
            scannedImageGraphics.ReleaseHdc(hdc);
            GlobalFree(dibhand);
            scannedImageGraphics.Dispose();
            bitdepth = binfo.biBitCount;
            scannedImage.SafeSetResolution(resx, resy);
            return scannedImage;
        }

        //THIS METHOD GETS THE POINTER TO THE BITMAP HEADER INFO
        private static IntPtr GetPixelInfo(IntPtr bmpPtr)
        {
            var bmi = (BitmapInfoHeader)Marshal.PtrToStructure(bmpPtr, typeof(BitmapInfoHeader));

            if (bmi.biSizeImage == 0)
                bmi.biSizeImage = (uint)(((((bmi.biWidth * bmi.biBitCount) + 31) & ~31) >> 3) * bmi.biHeight);

            var p = (int)bmi.biClrUsed;
            if ((p == 0) && (bmi.biBitCount <= 8))
                p = 1 << bmi.biBitCount;
            p = (p * 4) + (int)bmi.biSize + (int)bmpPtr;
            return (IntPtr)p;
        }

        //THIS METHOD GETS THE POINTER TO THE BITMAP HEADER INFO
        private static BitmapInfoHeader GetDibInfo(IntPtr bmpPtr)
        {
            var bmi = (BitmapInfoHeader)Marshal.PtrToStructure(bmpPtr, typeof(BitmapInfoHeader));
            return bmi;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct BitmapInfoHeader
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;

        public void Init()
        {
            biSize = (uint)Marshal.SizeOf(this);
        }
    }
}
