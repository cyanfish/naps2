/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2015       Luca De Petrillo

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using NAPS2.Lang.Resources;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfImageSettings
    {
        public const int DEFAULT_JPEG_QUALITY = 75;
        public const bool DEFAULT_COMPRESS_IMAGES = false;

        public PdfImageSettings()
        {
            JpegQuality = DEFAULT_JPEG_QUALITY;
            CompressImages = DEFAULT_COMPRESS_IMAGES;
        }

        public int JpegQuality { get; set; }

        public bool CompressImages { get; set; }
    }
}