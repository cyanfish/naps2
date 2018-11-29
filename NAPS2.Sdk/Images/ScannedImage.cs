using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Recovery;
using NAPS2.Scan;

namespace NAPS2.Images
{
    public class ScannedImage : IDisposable
    {
        private IImage thumbnail;
        private int thumbnailState;
        private int transformState;

        private bool disposed;
        private int snapshotCount;
        
        public ScannedImage(IStorage storage) : this(storage, new StorageConvertParams())
        {
        }

        public ScannedImage(IStorage storage, StorageConvertParams convertParams)
        {
            BackingStorage = StorageManager.ConvertToBacking(storage, convertParams);
            Metadata = StorageManager.ImageMetadataFactory.CreateMetadata(BackingStorage);
            Metadata.Commit();
        }

        public ScannedImage(IStorage storage, IImageMetadata metadata, StorageConvertParams convertParams)
        {
            BackingStorage = StorageManager.ConvertToBacking(storage, convertParams);
            Metadata = metadata;
        }

        public ScannedImage(IStorage storage, ScanBitDepth bitDepth, bool highQuality, int quality)
        {
            BackingStorage = StorageManager.ConvertToBacking(storage, new StorageConvertParams { Lossless = highQuality, LossyQuality = quality });
            Metadata = StorageManager.ImageMetadataFactory.CreateMetadata(BackingStorage);
            // TODO: Is this stuff really needed in metadata?
            Metadata.BitDepth = bitDepth;
            Metadata.Lossless = highQuality;
            Metadata.Commit();
        }

        public IStorage BackingStorage { get; }

        public IImageMetadata Metadata { get; }

        public PatchCode PatchCode { get; set; }

        public void Dispose()
        {
            lock (this)
            {
                disposed = true;
                // TODO: Does this work as intended? Since the recovery image isn't removed from the index
                if (snapshotCount != 0) return;

                // Delete the image data on disk
                BackingStorage?.Dispose();
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
                if (!Transform.AddOrSimplify(Metadata.TransformList, transform))
                {
                    return;
                }
                transformState++;
            }
            Metadata.Commit();
            ThumbnailInvalidated?.Invoke(this, new EventArgs());
        }

        public void ResetTransforms()
        {
            lock (this)
            {
                if (Metadata.TransformList.Count == 0)
                {
                    return;
                }
                Metadata.TransformList.Clear();
                transformState++;
            }
            Metadata.Commit();
            ThumbnailInvalidated?.Invoke(this, new EventArgs());
        }

        public IImage GetThumbnail()
        {
            lock (this)
            {
                return thumbnail?.Clone();
            }
        }

        public void SetThumbnail(IImage image, int? state = null)
        {
            lock (this)
            {
                thumbnail?.Dispose();
                thumbnail = image;
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
            Metadata.Index = index;
            Metadata.Commit();
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
                    TransformList = source.Metadata.TransformList.ToList();
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
                // TODO
                //info.AddValue("RecoveryIndexImage", Source.RecoveryIndexImage);
                info.AddValue("TransformList", TransformList);
                info.AddValue("TransformState", TransformState);
            }

            private Snapshot(SerializationInfo info, StreamingContext context)
            {
                // TODO
                //Source = new ScannedImage((RecoveryIndexImage)info.GetValue("RecoveryIndexImage", typeof(RecoveryIndexImage)));
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
