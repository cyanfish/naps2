using System;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.Images
{
    public class ScannedImage : IDisposable
    {
        private IImage? _thumbnail;
        private int _thumbnailState;

        private BarcodeDetection _barcodeDetection = BarcodeDetection.NotAttempted;
        private bool _disposed;
        private int _snapshotCount;

        public ScannedImage(IStorage backingStorage, IImageMetadata metadata)
        {
            BackingStorage = backingStorage;
            Metadata = metadata;
        }

        public IStorage BackingStorage { get; }

        public IImageMetadata Metadata { get; }

        public BarcodeDetection BarcodeDetection
        {
            get => _barcodeDetection;
            set => _barcodeDetection = value ?? throw new ArgumentNullException(nameof(value));
        }

        public void Dispose()
        {
            lock (this)
            {
                _disposed = true;
                // Delete the recovery entry (if recovery is being used)
                Metadata?.Dispose();
                
                // We defer deleting the actual data until all snapshots are disposed
                if (_snapshotCount != 0) return;

                // Delete the image data on disk
                BackingStorage?.Dispose();
                if (_thumbnail != null)
                {
                    _thumbnail.Dispose();
                    _thumbnail = null;
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

        public IImage? GetThumbnail()
        {
            lock (this)
            {
                return _thumbnail?.Clone();
            }
        }

        public void SetThumbnail(IImage image, int? state = null)
        {
            lock (this)
            {
                _thumbnail?.Dispose();
                _thumbnail = image;
                _thumbnailState = state ?? Metadata.TransformState;
            }
            ThumbnailChanged?.Invoke(this, new EventArgs());
        }

        public bool IsThumbnailDirty => _thumbnailState != Metadata.TransformState;

        public EventHandler? ThumbnailChanged;

        public EventHandler? ThumbnailInvalidated;

        public EventHandler? FullyDisposed;

        public Snapshot Preserve() => new Snapshot(this);

        public class Snapshot : IDisposable, IEquatable<Snapshot>
        {
            private readonly string _transformListXml;
            private bool _disposed;

            internal Snapshot(ScannedImage source)
            {
                lock (source)
                {
                    if (source._disposed)
                    {
                        throw new ObjectDisposedException("source");
                    }
                    source._snapshotCount++;
                    Source = source;
                    Metadata = source.Metadata.Clone();
                    _transformListXml = Metadata.TransformList.ToXml();
                }
            }

            public ScannedImage Source { get; }

            public IImageMetadata Metadata { get; }

            public void Dispose()
            {
                if (_disposed) return;
                lock (Source)
                {
                    _disposed = true;
                    Source._snapshotCount--;
                    if (Source._disposed && Source._snapshotCount == 0)
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
                       && Equals(_transformListXml, other._transformListXml);
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
                    hashCode = (hashCode * 397) ^ _transformListXml.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator ==(Snapshot left, Snapshot right) => Equals(left, right);

            public static bool operator !=(Snapshot left, Snapshot right) => !Equals(left, right);
        }
    }
}
