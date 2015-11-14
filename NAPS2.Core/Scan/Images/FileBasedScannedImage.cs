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
using NAPS2.Recovery;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.Scan.Images
{
    public class FileBasedScannedImage : IScannedImage
    {
        public const string LOCK_FILE_NAME = ".lock";

        private static DirectoryInfo _recoveryFolder;
        private static FileInfo _recoveryLockFile;
        private static FileStream _recoveryLock;
        private static RecoveryIndexManager _recoveryIndexManager;

        public static bool DisableRecoveryCleanup { get; set; }

        private static DirectoryInfo RecoveryFolder
        {
            get
            {
                if (_recoveryFolder == null)
                {
                    _recoveryFolder = new DirectoryInfo(Path.Combine(Paths.Recovery, Path.GetRandomFileName()));
                    _recoveryFolder.Create();
                    _recoveryLockFile = new FileInfo(Path.Combine(_recoveryFolder.FullName, LOCK_FILE_NAME));
                    _recoveryLock = _recoveryLockFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    _recoveryIndexManager = new RecoveryIndexManager(_recoveryFolder);
                }
                return _recoveryFolder;
            }
        }

        private static int _recoveryFileNumber = 1;

        private Bitmap thumbnail;
        // Store the actual image on disk
        private readonly ImageFormat baseImageFileFormat;
        private readonly string baseImageFileName;
        private readonly string baseImageFilePath;
        // Store a base image and transform pair (rather than doing the actual transform on the base image)
        // so that JPEG degradation is minimized when multiple rotations/flips are performed
        private readonly List<Transform> transformList = new List<Transform>();

        public FileBasedScannedImage(Bitmap img, ScanBitDepth bitDepth, bool highQuality)
        {
            Bitmap baseImage;
            MemoryStream baseImageEncoded;
            ScannedImageHelper.GetSmallestBitmap(img, bitDepth, highQuality, out baseImage, out baseImageEncoded, out baseImageFileFormat);

            baseImageFileName = (_recoveryFileNumber++).ToString("D5", CultureInfo.InvariantCulture) + GetExtension(baseImageFileFormat);
            baseImageFilePath = Path.Combine(RecoveryFolder.FullName, baseImageFileName);

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

            _recoveryIndexManager.Index.Images.Add(new RecoveryIndexImage
            {
                FileName = baseImageFileName,
                BitDepth = bitDepth,
                HighQuality = highQuality,
                TransformList = transformList
            });
            _recoveryIndexManager.Save();
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

        public Bitmap GetImage()
        {
            var bitmap = new Bitmap(baseImageFilePath);
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
            try
            {
                if (!DisableRecoveryCleanup && File.Exists(baseImageFilePath))
                {
                    File.Delete(baseImageFilePath);
                    _recoveryIndexManager.Index.Images.RemoveAll(x => x.FileName == baseImageFileName);
                    _recoveryIndexManager.Save();
                    if (_recoveryIndexManager.Index.Images.Count == 0)
                    {
                        _recoveryLock.Dispose();
                        RecoveryFolder.Delete(true);
                        _recoveryFolder = null;
                    }
                }
                if (thumbnail != null)
                {
                    thumbnail.Dispose();
                }
            }
            catch (IOException ex)
            {
                Log.ErrorException("Error cleaning up recovery files.", ex);
            }
        }

        public void AddTransform(Transform transform)
        {
            // Also updates the recovery index since they reference the same list
            Transform.AddOrSimplify(transformList, transform);
            _recoveryIndexManager.Save();
        }

        public void ResetTransforms()
        {
            transformList.Clear();
            _recoveryIndexManager.Save();
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
            var indexImage = _recoveryIndexManager.Index.Images.Single(x => x.FileName == baseImageFileName);
            _recoveryIndexManager.Index.Images.Remove(indexImage);
            _recoveryIndexManager.Index.Images.Insert(index, indexImage);
            _recoveryIndexManager.Save();
        }

        public PatchCode PatchCode { get; set; }
    }
}
