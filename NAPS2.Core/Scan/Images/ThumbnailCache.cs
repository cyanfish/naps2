using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Scan.Images
{
    public class ThumbnailCache : IDisposable
    {
        private readonly Dictionary<ScannedImage, CacheItem> cache = new Dictionary<ScannedImage, CacheItem>();

        private readonly ThumbnailRenderer thumbnailRenderer;

        public ThumbnailCache(ThumbnailRenderer thumbnailRenderer)
        {
            this.thumbnailRenderer = thumbnailRenderer;
        }

        public Bitmap this[ScannedImage scannedImage]
        {
            get
            {
                var newState = scannedImage.GetThumbnailState();
                if (cache.ContainsKey(scannedImage))
                {
                    // Cache hit
                    var item = cache[scannedImage];
                    if (item.State != newState)
                    {
                        // Invalidated
                        item.Thumbnail.Dispose();
                        item.Thumbnail = scannedImage.GetThumbnail(thumbnailRenderer);
                        item.State = newState;
                    }
                    return item.Thumbnail;
                }
                else
                {
                    // Cache miss
                    var item = new CacheItem
                    {
                        Thumbnail = scannedImage.GetThumbnail(thumbnailRenderer),
                        State = newState
                    };
                    return item.Thumbnail;
                }
            }
        }

        public void TrimCache(IEnumerable<ScannedImage> currentImages)
        {
            foreach (var key in cache.Keys.Except(currentImages).ToList())
            {
                cache[key].Thumbnail.Dispose();
                cache.Remove(key);
            }
        }

        private class CacheItem
        {
            public Bitmap Thumbnail { get; set; }

            public object State { get; set; }
        }

        #region IDisposable Support

        private bool disposed; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                foreach (var item in cache.Values) item.Thumbnail.Dispose();
                cache.Clear();
                disposed = true;
            }
        }

        ~ThumbnailCache()
        {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose() => Dispose(true);

        #endregion IDisposable Support
    }
}