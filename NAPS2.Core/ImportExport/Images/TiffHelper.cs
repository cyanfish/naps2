/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NAPS2.Scan.Images;

namespace NAPS2.ImportExport.Images
{
    class TiffHelper
    {
        public static bool SaveMultipage(List<IScannedImage> images, string location, Func<int, bool> progressCallback)
        {
            try
            {
                ImageCodecInfo codecInfo = GetCodecForString("TIFF");

                if (!progressCallback(0))
                {
                    return false;
                }

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
