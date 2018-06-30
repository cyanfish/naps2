using NAPS2.Scan.Images;
using NAPS2.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace NAPS2.ImportExport.Images
{
    public class TiffHelper
    {
        private readonly ScannedImageRenderer scannedImageRenderer;

        public TiffHelper(ScannedImageRenderer scannedImageRenderer)
        {
            this.scannedImageRenderer = scannedImageRenderer;
        }

        public bool SaveMultipage(List<ScannedImage> images, string location, TiffCompression compression, Func<int, bool> progressCallback)
        {
            try
            {
                ImageCodecInfo codecInfo = GetCodecForString("TIFF");

                if (!progressCallback(0))
                {
                    return false;
                }

                PathHelper.EnsureParentDirExists(location);

                if (images.Count == 1)
                {
                    var iparams = new EncoderParameters(1);
                    Encoder iparam = Encoder.Compression;
                    using (var bitmap = scannedImageRenderer.Render(images[0]))
                    {
                        ValidateBitmap(bitmap);
                        var iparamPara = new EncoderParameter(iparam, (long)GetEncoderValue(compression, bitmap));
                        iparams.Param[0] = iparamPara;
                        bitmap.Save(location, codecInfo, iparams);
                    }
                }
                else if (images.Count > 1)
                {
                    var encoderParams = new EncoderParameters(2);
                    var saveEncoder = Encoder.SaveFlag;
                    var compressionEncoder = Encoder.Compression;
                    File.Delete(location);
                    // TODO: https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2202-do-not-dispose-objects-multiple-times
                    using (var bitmap0 = scannedImageRenderer.Render(images[0]))
                    {
                        ValidateBitmap(bitmap0);
                        encoderParams.Param[0] = new EncoderParameter(compressionEncoder, (long)GetEncoderValue(compression, bitmap0));
                        encoderParams.Param[1] = new EncoderParameter(saveEncoder, (long)EncoderValue.MultiFrame);
                        bitmap0.Save(location, codecInfo, encoderParams);

                        for (int i = 1; i < images.Count; i++)
                        {
                            if (images[i] == null)
                                break;

                            if (!progressCallback(i))
                            {
                                bitmap0.Dispose();
                                File.Delete(location);
                                return false;
                            }

                            using (var bitmap = scannedImageRenderer.Render(images[i]))
                            {
                                ValidateBitmap(bitmap);
                                encoderParams.Param[0] = new EncoderParameter(compressionEncoder, (long)GetEncoderValue(compression, bitmap));
                                encoderParams.Param[1] = new EncoderParameter(saveEncoder, (long)EncoderValue.FrameDimensionPage);
                                bitmap0.SaveAdd(bitmap, encoderParams);
                            }
                        }

                        encoderParams.Param[0] = new EncoderParameter(saveEncoder, (long)EncoderValue.Flush);
                        bitmap0.SaveAdd(encoderParams);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving TIFF", ex);
            }
        }

        private EncoderValue GetEncoderValue(TiffCompression compression, Bitmap bitmap)
        {
            switch (compression)
            {
                case TiffCompression.None:
                    return EncoderValue.CompressionNone;

                case TiffCompression.Ccitt4:
                    return EncoderValue.CompressionCCITT4;

                case TiffCompression.Lzw:
                    return EncoderValue.CompressionLZW;

                default:
                    if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed
                        && bitmap.Palette.Entries.Length == 2
                        && bitmap.Palette.Entries[0].ToArgb() == Color.Black.ToArgb()
                        && bitmap.Palette.Entries[1].ToArgb() == Color.White.ToArgb())
                    {
                        return EncoderValue.CompressionCCITT4;
                    }
                    else
                    {
                        return EncoderValue.CompressionLZW;
                    }
            }
        }

        private void ValidateBitmap(Bitmap bitmap)
        {
            if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed
                && bitmap.Palette.Entries.Length == 2
                && bitmap.Palette.Entries[0].ToArgb() == Color.White.ToArgb()
                && bitmap.Palette.Entries[1].ToArgb() == Color.Black.ToArgb())
            {
                // Inverted palette (0 = white); some scanners may produce bitmaps like this
                // It won't encode properly in a TIFF, so we need to invert the encoding
                var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                var stride = Math.Abs(data.Stride);
                for (int y = 0; y < data.Height; y++)
                {
                    for (int x = 0; x < data.Width; x += 8)
                    {
                        byte b = Marshal.ReadByte(data.Scan0 + (y * stride) + (x / 8));
                        int bits = Math.Min(8, data.Width - x);
                        b ^= (byte)(0xFF << (8 - bits));
                        Marshal.WriteByte(data.Scan0 + (y * stride) + (x / 8), b);
                    }
                }
                bitmap.UnlockBits(data);
                bitmap.Palette.Entries[0] = Color.Black;
                bitmap.Palette.Entries[1] = Color.White;
            }
        }

        private ImageCodecInfo GetCodecForString(string type)
        {
            ImageCodecInfo[] info = ImageCodecInfo.GetImageEncoders();
            return Array.Find(info, t => t.FormatDescription.Equals(type));
        }
    }
}