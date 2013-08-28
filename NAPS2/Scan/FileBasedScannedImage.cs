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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using NLog;

namespace NAPS2.Scan
{
    public class FileBasedScannedImage : IScannedImage
    {
        private static DirectoryInfo _recoveryFolder;

        private static DirectoryInfo RecoveryFolder
        {
            get
            {
                if (_recoveryFolder == null)
                {
                    _recoveryFolder = new DirectoryInfo(Path.Combine(Paths.Recovery, Path.GetRandomFileName()));
                    _recoveryFolder.Create();
                }
                return _recoveryFolder;
            }
        }

        private static int _recoveryFileNumber = 1;
        private static int _recoveryImageCount = 0;

        private readonly Logger logger;

        // The image's bit depth (or C24Bit if unknown)
        private readonly ScanBitDepth bitDepth;
        // Store the actual image on disk
        private readonly ImageFormat baseImageFileFormat;
        private readonly string baseImageFilePath;
        // Store a base image and transform pair (rather than doing the actual transform on the base image)
        // so that JPEG degradation is minimized when multiple rotations/flips are performed
        private RotateFlipType transform = RotateFlipType.RotateNoneFlipNone;

        public FileBasedScannedImage(Bitmap img, ScanBitDepth bitDepth, bool highQuality, Logger logger)
        {
            this.bitDepth = bitDepth;
            this.logger = logger;
            Thumbnail = ThumbnailHelper.GetThumbnail(img);

            Bitmap baseImage;
            MemoryStream baseImageEncoded;
            ScannedImageHelper.GetSmallestBitmap(img, bitDepth, highQuality, out baseImage, out baseImageEncoded, out baseImageFileFormat);

            baseImageFilePath = Path.Combine(RecoveryFolder.FullName, (_recoveryFileNumber++).ToString("D5", CultureInfo.InvariantCulture)) + GetExtension(baseImageFileFormat);

            if (baseImage != null)
            {
                // TODO: If I'm stuck using PNG anyway, then don't treat B&W specially
                baseImage.Save(baseImageFilePath, baseImageFileFormat);
                baseImage.Dispose();
            }
            else
            {
                Debug.Assert(baseImageEncoded != null);
                using (var fs = new FileStream(baseImageFilePath, FileMode.CreateNew))
                {
                    baseImageEncoded.Seek(0, SeekOrigin.Begin);
                    baseImageEncoded.CopyTo(fs);
                }
                baseImageEncoded.Dispose();
            }

            _recoveryImageCount++;
        }

        private string GetExtension(ImageFormat imageFormat)
        {
            if (Equals(imageFormat, ImageFormat.Png))
            {
                return ".png";
            }
            if (Equals(imageFormat, ImageFormat.Jpeg))
            {
                return ".jpg";
            }
            throw new ArgumentException();
        }

        public Bitmap Thumbnail { get; private set; }

        public Bitmap GetImage()
        {
            var bitmap = new Bitmap(baseImageFilePath);
            bitmap.RotateFlip(transform);
            return bitmap;
        }

        public void Dispose()
        {
            Thumbnail.Dispose();
            try
            {
                if (File.Exists(baseImageFilePath))
                {
                    File.Delete(baseImageFilePath);
                    _recoveryImageCount--;
                    if (_recoveryImageCount == 0)
                    {
                        RecoveryFolder.Delete(true);
                    }
                }
            }
            catch (IOException ex)
            {
                logger.ErrorException("Error cleaning up recovery files.", ex);
            }
        }

        public void RotateFlip(RotateFlipType rotateFlipType)
        {
            // There should be no actual flips (just rotations of varying degrees), so this code is simplified
            transform = TransformationHelper.CombineRotation(transform, rotateFlipType);
            Thumbnail.RotateFlip(rotateFlipType);
        }
    }
}
