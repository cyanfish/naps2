using System;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.Images
{
    public class ScannedImage : IDisposable
    {
        private IImage thumbnail;
        private int thumbnailState;

        private bool disposed;
        private int snapshotCount;

        public ScannedImage(IStorage backingStorage, IImageMetadata metadata)
        {
            BackingStorage = backingStorage;
            Metadata = metadata;
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
                // Delete the recovery entry (if recovery is being used)
                Metadata?.Dispose();
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
            if (transform.IsNull)
            {
                return;
            }
            lock (this)
            {
                // Also updates the recovery index since they reference the same list
                Transform.AddOrSimplify(Metadata.TransformList, transform);
                Metadata.TransformState++;
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
                Metadata.TransformState++;
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
                thumbnailState = state ?? Metadata.TransformState;
            }
            ThumbnailChanged?.Invoke(this, new EventArgs());
        }

        public bool IsThumbnailDirty => thumbnailState != Metadata.TransformState;

        public EventHandler ThumbnailChanged;

        public EventHandler ThumbnailInvalidated;

        public EventHandler FullyDisposed;

        // TODO: Not using this any more, need to find a better way
        public void MovedTo(int index)
        {
            Metadata.Index = index;
            Metadata.Commit();
        }

        public Snapshot Preserve() => new Snapshot(this);

        public class Snapshot : IDisposable, IEquatable<Snapshot>
        {
            private readonly string transformListXml;
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
                    Metadata = source.Metadata.Clone();
                    transformListXml = Metadata.TransformList.ToXml();
                }
            }

            public ScannedImage Source { get; }

            public IImageMetadata Metadata { get; }

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

            public bool Equals(Snapshot other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(Source, other.Source)
                       && Equals(Metadata.Lossless, other.Metadata.Lossless)
                       && Equals(Metadata.BitDepth, other.Metadata.BitDepth)
                       && Equals(transformListXml, other.transformListXml);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Snapshot) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Source.GetHashCode();
                    hashCode = (hashCode * 397) ^ Metadata.Lossless.GetHashCode();
                    hashCode = (hashCode * 397) ^ Metadata.BitDepth.GetHashCode();
                    hashCode = (hashCode * 397) ^ transformListXml.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator ==(Snapshot left, Snapshot right) => Equals(left, right);

            public static bool operator !=(Snapshot left, Snapshot right) => !Equals(left, right);
        }
    }
}
