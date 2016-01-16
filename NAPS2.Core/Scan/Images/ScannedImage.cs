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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NAPS2.Recovery;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.Scan.Images
{
    public class ScannedImage : IDisposable
    {
        private readonly RecoveryImage recoveryImage;

        // Store a base image and transform pair (rather than doing the actual transform on the base image)
        // so that JPEG degradation is minimized when multiple rotations/flips are performed
        private readonly List<Transform> transformList;

        private Bitmap thumbnail;

        public ScannedImage(Bitmap img, ScanBitDepth bitDepth, bool highQuality, int quality)
        {
            Bitmap baseImage;
            MemoryStream baseImageEncoded;
            ImageFormat baseImageFileFormat;
            ScannedImageHelper.GetSmallestBitmap(img, bitDepth, highQuality, quality, out baseImage, out baseImageEncoded, out baseImageFileFormat);

            transformList = new List<Transform>();
            recoveryImage = RecoveryImage.CreateNew(baseImageFileFormat, bitDepth, highQuality, transformList);

            if (baseImage != null)
            {
                baseImage.Save(recoveryImage.FilePath, recoveryImage.FileFormat);
                baseImage.Dispose();
            }
            else
            {
                Debug.Assert(baseImageEncoded != null);
                using (var fs = new FileStream(recoveryImage.FilePath, FileMode.CreateNew))
                {
                    baseImageEncoded.Seek(0, SeekOrigin.Begin);
                    baseImageEncoded.CopyTo(fs);
                }
                baseImageEncoded.Dispose();
            }

            recoveryImage.Save();
        }

        public ScannedImage(RecoveryIndexImage recoveryIndexImage)
        {
            recoveryImage = RecoveryImage.LoadExisting(recoveryIndexImage);
            transformList = recoveryImage.IndexImage.TransformList;
        }

        public PatchCode PatchCode { get; set; }

        public ImageFormat FileFormat { get { return recoveryImage.FileFormat; } }

        internal RecoveryIndexImage RecoveryIndexImage
        {
            get
            {
                return recoveryImage.IndexImage;
            }
        }

        public Bitmap GetImage()
        {
            var bitmap = new Bitmap(recoveryImage.FilePath);
            return Transform.PerformAll(bitmap, transformList);
        }

        public Stream GetImageStream()
        {
            using (var transformed = GetImage())
            {
                var stream = new MemoryStream();
                transformed.Save(stream, recoveryImage.FileFormat);
                return stream;
            }
        }

        public void Dispose()
        {
            if (recoveryImage != null)
            {
                recoveryImage.Dispose();
            }
            if (thumbnail != null)
            {
                thumbnail.Dispose();
            }
        }

        public void AddTransform(Transform transform)
        {
            // Also updates the recovery index since they reference the same list
            Transform.AddOrSimplify(transformList, transform);
            recoveryImage.Save();
        }

        public void ResetTransforms()
        {
            transformList.Clear();
            recoveryImage.Save();
        }

        public Bitmap GetThumbnail(int preferredSize)
        {
            if (thumbnail == null)
            {
                thumbnail = RenderThumbnail(preferredSize);
            }
            Debug.Assert(thumbnail != null);
            return (Bitmap)thumbnail.Clone();
        }

        public object GetThumbnailState()
        {
            return thumbnail;
        }

        public void SetThumbnail(Bitmap bitmap)
        {
            if (thumbnail != null)
            {
                thumbnail.Dispose();
            }
            thumbnail = bitmap;
        }

        public Bitmap RenderThumbnail(int size)
        {
            using (var img = GetImage())
            {
                return ThumbnailHelper.GetThumbnail(img, size);
            }
        }

        public void MovedTo(int index)
        {
            recoveryImage.Move(index);
        }
    }
}
