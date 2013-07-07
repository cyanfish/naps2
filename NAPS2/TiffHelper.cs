/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

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

namespace NAPS2
{
    class TiffHelper
    {
        public static bool SaveMultipage(Image[] bmp, string location)
        {
            if (bmp != null)
            {
                try
                {
                    ImageCodecInfo codecInfo = GetCodecForString("TIFF");

                    if (bmp.Length == 1)
                    {

                        var iparams = new EncoderParameters(1);
                        Encoder iparam = Encoder.Compression;
                        var iparamPara = new EncoderParameter(iparam, (long)(EncoderValue.CompressionLZW));
                        iparams.Param[0] = iparamPara;
                        bmp[0].Save(location, codecInfo, iparams);


                    }
                    else if (bmp.Length > 1)
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
                        bmp[0].Save(location, codecInfo, encoderParams);


                        for (int i = 1; i < bmp.Length; i++)
                        {
                            if (bmp[i] == null)
                                break;

                            SaveEncodeParam = new EncoderParameter(saveEncoder, (long)EncoderValue.FrameDimensionPage);
                            CompressionEncodeParam = new EncoderParameter(compressionEncoder, (long)EncoderValue.CompressionLZW);
                            encoderParams.Param[0] = CompressionEncodeParam;
                            encoderParams.Param[1] = SaveEncodeParam;
                            bmp[0].SaveAdd(bmp[i], encoderParams);

                        }

                        SaveEncodeParam = new EncoderParameter(saveEncoder, (long)EncoderValue.Flush);
                        encoderParams.Param[0] = SaveEncodeParam;
                        bmp[0].SaveAdd(encoderParams);
                    }
                    return true;


                }
                catch (Exception ee)
                {
                    throw new Exception(ee.Message + "  Error in saving as multipage ");
                }
            }
            else
                return false;

        }
        private static ImageCodecInfo GetCodecForString(string type)
        {
            ImageCodecInfo[] info = ImageCodecInfo.GetImageEncoders();

            for (int i = 0; i < info.Length; i++)
            {
                string enumName = type;
                if (info[i].FormatDescription.Equals(enumName))
                {
                    return info[i];
                }
            }

            return null;

        }
    }
}
