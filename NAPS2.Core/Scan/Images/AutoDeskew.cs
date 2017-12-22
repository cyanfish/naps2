using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.Scan.Images
{
    public static class AutoDeskewExtensions
    {
        public static double GetSkewAngle(this Bitmap bmp)
        {
            var sk = new gmseDeskew(bmp);
            return sk.GetSkewAngle();
        }

        public static double GetSkewAngle(this Image img)
        {
            var bmp = new Bitmap(img, img.Width, img.Height);
            var res = bmp.GetSkewAngle();
            bmp.Dispose();
            return res;
        }
    }

    // The following class "gmseDeskew" was released under Code Project Open License (CPOL) 1.02 by:
    // (c) GMSE GmbH 2006
    // Algorithm to deskew an image.
    // http://www.codeproject.com/Articles/13615/How-to-deskew-an-image
    //
    // Ported to C# by Peter Hommel in 2016
    // https://github.com/phommel
    public class gmseDeskew
    {
        // Representation of a line in the image.
        public class HougLine
        {
            // Count of points in the line.
            public int Count;
            // Index in Matrix.
            public int Index;
            // The line is represented as all x,y that solve y*cos(alpha)-x*sin(alpha)=d
            public double Alpha;
            public double d;
        }
        // The Bitmap
        int width;
        int height;
        int stride;
        byte[] bitmapBytes;
        PixelFormat pf;
        // The range of angles to search for lines
        readonly double cAlphaStart = -20;
        readonly double cAlphaStep = 0.2;
        int cSteps = 40 * 5;
        // Precalculation of sin and cos.
        double[] cSinA;
        double[] cCosA;
        // Range of d
        double cDMin;
        readonly double cDStep = 1;
        int cDCount;
        // Count of points that fit in a line.

        int[] cHMatrix;
        // Calculate the skew angle of the image cBmp.
        public double GetSkewAngle()
        {
            HougLine[] hl;
            int i;
            double sum = 0;
            int count = 0;

            // Hough Transformation
            Calc();
            // Top 20 of the detected lines in the image.
            hl = GetTop(20);
            // Average angle of the lines
            for (i = 0; i <= 19; i++)
            {
                sum += hl[i].Alpha;
                count += 1;
            }
            return sum / count;
        }

        // Calculate the Count lines in the image with most points.
        private HougLine[] GetTop(int Count)
        {
            HougLine[] hl;
            int i;
            int j;
            HougLine tmp;
            int AlphaIndex;
            int dIndex;

            hl = new HougLine[Count + 1];
            for (i = 0; i <= Count - 1; i++)
            {
                hl[i] = new HougLine();
            }
            for (i = 0; i <= cHMatrix.Length - 1; i++)
            {
                if (cHMatrix[i] > hl[Count - 1].Count)
                {
                    hl[Count - 1].Count = cHMatrix[i];
                    hl[Count - 1].Index = i;
                    j = Count - 1;
                    while (j > 0 && hl[j].Count > hl[j - 1].Count)
                    {
                        tmp = hl[j];
                        hl[j] = hl[j - 1];
                        hl[j - 1] = tmp;
                        j -= 1;
                    }
                }
            }
            for (i = 0; i <= Count - 1; i++)
            {
                dIndex = hl[i].Index / cSteps;
                AlphaIndex = hl[i].Index - dIndex * cSteps;
                hl[i].Alpha = GetAlpha(AlphaIndex);
                hl[i].d = dIndex + cDMin;
            }
            return hl;
        }

        public gmseDeskew(Bitmap bitmap)
        {
            width = bitmap.Width;
            height = bitmap.Height;
            pf = bitmap.PixelFormat;
            LoadBitmap(bitmap);
        }

        private void LoadBitmap(Bitmap bitmap)
        {
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            stride = Math.Abs(data.Stride);
            bitmapBytes = new byte[stride * data.Height];
            Marshal.Copy(data.Scan0, bitmapBytes, 0, bitmapBytes.Length);
            bitmap.UnlockBits(data);
        }

        // Hough Transforamtion:
        private void Calc()
        {
            Init();
            for(int y = 1; y <= height - 2; y++)
            {
                for (int x = 1; x <= width - 2; x++)
                {
                    // Only lower edges are considered.
                    if (IsBlack(x, y))
                    {
                        if (!IsBlack(x, y + 1))
                        {
                            Calc(x, y);
                        }
                    }
                }
            }
        }
        // Calculate all lines through the point (x,y).
        private void Calc(int x, int y)
        {
            int alpha;
            double d;
            int dIndex;
            int Index;

            for (alpha = 0; alpha <= cSteps - 1; alpha++)
            {
                d = y * cCosA[alpha] - x * cSinA[alpha];
                dIndex = (int)(d - cDMin);
                Index = dIndex * cSteps + alpha;
                try
                {
                    cHMatrix[Index]++;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
        private bool IsBlack(int x, int y)
        {
            if (pf == PixelFormat.Format1bppIndexed)
            {
                var b = bitmapBytes[y * stride + (x >> 3)];
                var mask = (byte)(0x80 >> (x & 0x7));
                return (b & mask) == 0;
            }
            else if (pf == PixelFormat.Format24bppRgb)
            {
                int r = bitmapBytes[stride * y + x * 3];
                int g = bitmapBytes[stride * y + x * 3 + 1];
                int b = bitmapBytes[stride * y + x * 3 + 2];
                double luminance = (r * 0.299) + (g * 0.587) + (b * 0.114);
                return luminance < 140;
            }
            else if (pf == PixelFormat.Format32bppArgb)
            {
                int r = bitmapBytes[stride * y + x * 4 + 1];
                int g = bitmapBytes[stride * y + x * 4 + 2];
                int b = bitmapBytes[stride * y + x * 4 + 3];
                double luminance = (r * 0.299) + (g * 0.587) + (b * 0.114);
                return luminance < 140;
            }
            else
            {
                throw new ArgumentException("Unsupported pixel format");
            }
        }
        private void Init()
        {
            int i;
            double angle;

            // Precalculation of sin and cos.
            cSinA = new double[cSteps];
            cCosA = new double[cSteps];
            for (i = 0; i <= cSteps - 1; i++)
            {
                angle = GetAlpha(i) * Math.PI / 180.0;
                cSinA[i] = Math.Sin(angle);
                cCosA[i] = Math.Cos(angle);
            }
            // Range of d:
            cDMin = -width;
            cDCount = (int)(2 * (width + height) / cDStep);
            cHMatrix = new int[cDCount * cSteps + 1];
        }

        public double GetAlpha(int Index)
        {
            return cAlphaStart + Index * cAlphaStep;
        }
    }
}
