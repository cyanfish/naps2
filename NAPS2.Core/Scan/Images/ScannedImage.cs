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
using NAPS2.Config;
using NAPS2.Scan.Images.Transforms;

namespace NAPS2.Scan.Images
{
    public class ScannedImage : IScannedImage
    {
        private readonly IUserConfigManager userConfigManager;
        // The image's bit depth (or C24Bit if unknown)
        private readonly ScanBitDepth bitDepth;
        // Only one of the following (baseImage/baseImageEncoded) should have a value for any particular ScannedImage
        private readonly Bitmap baseImage;
        private readonly MemoryStream baseImageEncoded;
        private readonly ImageFormat baseImageFileFormat;
        // Store a base image and transform pair (rather than doing the actual transform on the base image)
        // so that JPEG degradation is minimized when multiple rotations/flips are performed
        private readonly List<Transform> transformList = new List<Transform>();

        public ScannedImage(Bitmap img, ScanBitDepth bitDepth, bool highQuality, IUserConfigManager userConfigManager)
        {
            this.bitDepth = bitDepth;
            this.userConfigManager = userConfigManager;
            Thumbnail = ThumbnailHelper.GetThumbnail(img, userConfigManager.Config.ThumbnailSize);
            ScannedImageHelper.GetSmallestBitmap(img, bitDepth, highQuality, out baseImage, out baseImageEncoded, out baseImageFileFormat);
        }

        public Bitmap Thumbnail { get; private set; }

        public Bitmap GetImage()
        {
            var bitmap = bitDepth == ScanBitDepth.BlackWhite ? (Bitmap)baseImage.Clone() : new Bitmap(baseImageEncoded);
            return Transform.PerformAll(bitmap, transformList);
        }

        public Stream GetImageStream()
        {
            using (var transformed = GetImage())
            {
                var stream = new MemoryStream();
                transformed.Save(stream, baseImageFileFormat);
                return stream;
            }
        }

        public void Dispose()
        {
            if (baseImage != null)
            {
                baseImage.Dispose();
            }
            if (baseImageEncoded != null)
            {
                baseImageEncoded.Dispose();
            }
            Thumbnail.Dispose();
        }

        public void AddTransform(Transform transform)
        {
            Transform.AddOrSimplify(transformList, transform);
        }

        public void ResetTransforms()
        {
            transformList.Clear();
        }

        public void UpdateThumbnail()
        {
            using (var img = GetImage())
            {
                Thumbnail = ThumbnailHelper.GetThumbnail(img, userConfigManager.Config.ThumbnailSize);
            }
        }

        public void MovedTo(int index)
        {
            // Do nothing, this is only important for FileBasedScannedImage
        }
    }
}
