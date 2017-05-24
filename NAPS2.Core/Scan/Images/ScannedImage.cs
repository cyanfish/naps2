using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NAPS2.ImportExport.Pdf;
using NAPS2.Recovery;
using NAPS2.Scan.Images.Transforms;

namespace NAPS2.Scan.Images
{
    public class ScannedImage : IDisposable
    {
        // Store the base image and metadata on disk using a separate class to manage lifetime
        // If NAPS2 crashes, the image data can be recovered by the next instance of NAPS2 to start
        private readonly RecoveryImage recoveryImage;

        // Store a base image and transform pair (rather than doing the actual transform on the base image)
        // so that JPEG degradation is minimized when multiple rotations/flips are performed
        private readonly List<Transform> transformList;

        private Bitmap thumbnail;

        public static ScannedImage FromSinglePagePdf(string pdfPath, bool copy)
        {
            return new ScannedImage(pdfPath, copy);
        }

        public ScannedImage(Bitmap img, ScanBitDepth bitDepth, bool highQuality, int quality)
        {
            ImageFormat fileFormat;
            string tempFilePath = ScannedImageHelper.SaveSmallestBitmap(img, bitDepth, highQuality, quality, out fileFormat);

            transformList = new List<Transform>();
            recoveryImage = RecoveryImage.CreateNew(fileFormat, bitDepth, highQuality, transformList);

            File.Move(tempFilePath, recoveryImage.FilePath);

            recoveryImage.Save();
        }

        public ScannedImage(RecoveryIndexImage recoveryIndexImage)
        {
            recoveryImage = RecoveryImage.LoadExisting(recoveryIndexImage);
            transformList = recoveryImage.IndexImage.TransformList;
        }

        private ScannedImage(string pdfPath, bool copy)
        {
            transformList = new List<Transform>();
            recoveryImage = RecoveryImage.CreateNew(null, ScanBitDepth.C24Bit, false, transformList);

            if (copy)
            {
                File.Copy(pdfPath, recoveryImage.FilePath);
            }
            else
            {
                File.Move(pdfPath, recoveryImage.FilePath);
            }

            recoveryImage.Save();
        }

        public PatchCode PatchCode { get; set; }

        public ImageFormat FileFormat => recoveryImage.FileFormat;

        public RecoveryIndexImage RecoveryIndexImage => recoveryImage.IndexImage;

        public string RecoveryFilePath => recoveryImage.FilePath;

        public long Size => new FileInfo(recoveryImage.FilePath).Length;

        public void Dispose()
        {
            lock (this)
            {
                // Delete the image data on disk
                recoveryImage?.Dispose();
                if (thumbnail != null)
                {
                    thumbnail.Dispose();
                    thumbnail = null;
                }
            }
        }

        public void AddTransform(Transform transform)
        {
            lock (transformList)
            {
                // Also updates the recovery index since they reference the same list
                Transform.AddOrSimplify(transformList, transform);
            }
            recoveryImage.Save();
        }

        public void ResetTransforms()
        {
            lock (transformList)
            {
                transformList.Clear();
            }
            recoveryImage.Save();
        }

        public Bitmap GetThumbnail(ThumbnailRenderer thumbnailRenderer)
        {
            if (thumbnail == null)
            {
                if (thumbnailRenderer == null)
                {
                    return null;
                }
                thumbnail = thumbnailRenderer.RenderThumbnail(this);
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
            thumbnail?.Dispose();
            thumbnail = bitmap;
        }

        public void MovedTo(int index)
        {
            recoveryImage.Move(index);
        }
    }
}
