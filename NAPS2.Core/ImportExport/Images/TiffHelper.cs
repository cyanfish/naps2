using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport.Images
{
    class TiffHelper
    {
        public static bool SaveMultipage(List<ScannedImage> images, string location, Func<int, bool> progressCallback)
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
                    using (var bitmap = images[0].GetImage())
                    {
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
                    using (var bitmap0 = images[0].GetImage())
                    {
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
                            using (var bitmap = images[i].GetImage())
                            {
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
        private static ImageCodecInfo GetCodecForString(string type)
        {
            ImageCodecInfo[] info = ImageCodecInfo.GetImageEncoders();
            return info.FirstOrDefault(t => t.FormatDescription.Equals(type));
        }
    }
}
