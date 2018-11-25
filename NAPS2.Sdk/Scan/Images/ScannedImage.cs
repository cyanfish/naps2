using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using NAPS2.Recovery;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

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
        private int thumbnailState;
        private int transformState;

        private bool disposed;
        private int snapshotCount;

        public static ScannedImage FromSinglePagePdf(string pdfPath, bool copy)
        {
            return new ScannedImage(pdfPath, copy);
        }

        public ScannedImage(Bitmap img, ScanBitDepth bitDepth, bool highQuality, int quality)
        {
            string tempFilePath = ScannedImageHelper.SaveSmallestBitmap(img, bitDepth, highQuality, quality, out ImageFormat fileFormat);

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
                disposed = true;
                // TODO: Does this work as intended? Since the recovery image isn't removed from the index
                if (snapshotCount != 0) return;

                // Delete the image data on disk
                recoveryImage?.Dispose();
                if (thumbnail != null)
                {
                    thumbnail.Dispose();
                    thumbnail = null;
                }

                FullyDisposed?.Invoke(this, new EventArgs());
            }
        }

        public void AddTransform(Transform transform)
        {
            lock (this)
            {
                // Also updates the recovery index since they reference the same list
                if (!Transform.AddOrSimplify(transformList, transform))
                {
                    return;
                }
                transformState++;
            }
            recoveryImage.Save();
            ThumbnailInvalidated?.Invoke(this, new EventArgs());
        }

        public void ResetTransforms()
        {
            lock (this)
            {
                if (transformList.Count == 0)
                {
                    return;
                }
                transformList.Clear();
                transformState++;
            }
            recoveryImage.Save();
            ThumbnailInvalidated?.Invoke(this, new EventArgs());
        }

        public Bitmap GetThumbnail()
        {
            lock (this)
            {
                return (Bitmap) thumbnail?.Clone();
            }
        }

        public void SetThumbnail(Bitmap bitmap, int? state = null)
        {
            lock (this)
            {
                thumbnail?.Dispose();
                thumbnail = bitmap;
                thumbnailState = state ?? transformState;
            }
            ThumbnailChanged?.Invoke(this, new EventArgs());
        }

        public bool IsThumbnailDirty => thumbnailState != transformState;

        public EventHandler ThumbnailChanged;

        public EventHandler ThumbnailInvalidated;

        public EventHandler FullyDisposed;

        public void MovedTo(int index)
        {
            recoveryImage.Move(index);
        }

        public Snapshot Preserve() => new Snapshot(this);

        [Serializable]
        [KnownType("KnownTypes")]
        public class Snapshot : IDisposable, ISerializable
        {
            private bool disposed;

            internal Snapshot(ScannedImage source)
            {
                lock (source)
                {
                    if (source.disposed)
                    {
                        throw new ObjectDisposedException("source");
                    }
                    source.snapshotCount++;
                    Source = source;
                    TransformList = source.transformList.ToList();
                    TransformState = source.transformState;
                }
            }

            public ScannedImage Source { get; }

            public List<Transform> TransformList { get; }

            public int TransformState { get; }

            public void Dispose()
            {
                if (disposed) return;
                lock (Source)
                {
                    disposed = true;
                    Source.snapshotCount--;
                    if (Source.disposed && Source.snapshotCount == 0)
                    {
                        Source.Dispose();
                    }
                }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("RecoveryIndexImage", Source.RecoveryIndexImage);
                info.AddValue("TransformList", TransformList);
                info.AddValue("TransformState", TransformState);
            }

            private Snapshot(SerializationInfo info, StreamingContext context)
            {
                Source = new ScannedImage((RecoveryIndexImage)info.GetValue("RecoveryIndexImage", typeof(RecoveryIndexImage)));
                TransformList = (List<Transform>)info.GetValue("TransformList", typeof(List<Transform>));
                TransformState = (int)info.GetValue("TransformState", typeof(int));
            }

            // ReSharper disable once UnusedMember.Local
            private static Type[] KnownTypes()
            {
                var transformTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(Transform)));
                return transformTypes.Concat(new[] { typeof(List<Transform>), typeof(RecoveryIndexImage) }).ToArray();
            }
        }
    }
}
