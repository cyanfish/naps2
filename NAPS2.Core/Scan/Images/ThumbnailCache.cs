using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NAPS2.Config;

namespace NAPS2.Scan.Images
{
    public class ThumbnailCache : IDisposable
    {
        private readonly Dictionary<ScannedImage, CacheItem> cache = new Dictionary<ScannedImage,CacheItem>();

        private readonly ThumbnailRenderer thumbnailRenderer;

        public ThumbnailCache(ThumbnailRenderer thumbnailRenderer)
        {
            this.thumbnailRenderer = thumbnailRenderer;
        }

        ~ThumbnailCache()
        {
            Dispose();
        }

        public void Dispose()
        {
            foreach (var item in cache.Values)
            {
                item.Thumbnail.Dispose();
            }
            cache.Clear();
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
    }
}
