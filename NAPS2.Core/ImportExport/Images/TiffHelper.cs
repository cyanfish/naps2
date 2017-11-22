using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport.Images
{
    public class TiffHelper
    {
        private readonly ScannedImageRenderer scannedImageRenderer;

        public TiffHelper(ScannedImageRenderer scannedImageRenderer)
        {
            this.scannedImageRenderer = scannedImageRenderer;
        }

        public bool SaveMultipage(List<ScannedImage> images, string location, Func<int, bool> progressCallback)
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
                    var iparamPara = new EncoderParameter(iparam, (long)(EncoderValue.CompressionLZW));
                    iparams.Param[0] = iparamPara;
                    using (var bitmap = scannedImageRenderer.Render(images[0]))
                    {
                        ValidateBitmap(bitmap);
                        bitmap.Save(location, codecInfo, iparams);
                    }
                }
                else if (images.Count > 1)
                {
                    Encoder saveEncoder;
                    Encoder compressionEncoder;
                    EncoderParameter SaveEncodeParam;
                    EncoderParameter CompressionEncodeParam;
                    var encoderParams = new EncoderParameters(2);

                    saveEncoder = Encoder.SaveFlag;
                    compressionEncoder = Encoder.Compression;

                    // Save the first page (frame).
                    SaveEncodeParam = new EncoderParameter(saveEncoder, (long)EncoderValue.MultiFrame);
                    CompressionEncodeParam = new EncoderParameter(compressionEncoder, (long)EncoderValue.CompressionLZW);
                    encoderParams.Param[0] = CompressionEncodeParam;
                    encoderParams.Param[1] = SaveEncodeParam;

                    File.Delete(location);
                    using (var bitmap0 = scannedImageRenderer.Render(images[0]))
                    {
                        ValidateBitmap(bitmap0);
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

                            SaveEncodeParam = new EncoderParameter(saveEncoder, (long) EncoderValue.FrameDimensionPage);
                            CompressionEncodeParam = new EncoderParameter(compressionEncoder,
                                (long) EncoderValue.CompressionLZW);
                            encoderParams.Param[0] = CompressionEncodeParam;
                            encoderParams.Param[1] = SaveEncodeParam;
                            using (var bitmap = scannedImageRenderer.Render(images[i]))
                            {
                                ValidateBitmap(bitmap);
                                bitmap0.SaveAdd(bitmap, encoderParams);
                            }
                        }

                        SaveEncodeParam = new EncoderParameter(saveEncoder, (long) EncoderValue.Flush);
                        encoderParams.Param[0] = SaveEncodeParam;
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
                        byte b = Marshal.ReadByte(data.Scan0 + y * stride + x / 8);
                        int bits = Math.Min(8, data.Width - x);
                        b ^= (byte)(0xFF << (8 - bits));
                        Marshal.WriteByte(data.Scan0 + y * stride + x / 8, b);
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
            return info.FirstOrDefault(t => t.FormatDescription.Equals(type));
        }
    }
}
