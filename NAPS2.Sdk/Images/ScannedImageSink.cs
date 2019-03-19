using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Util;

namespace NAPS2.Images
{
    public class ScannedImageSink
    {
        private readonly List<TaskCompletionSource<ScannedImage>> images = new List<TaskCompletionSource<ScannedImage>>
        {
            new TaskCompletionSource<ScannedImage>()
        };

        public bool Completed { get; private set; }

        public int ImageCount
        {
            get
            {
                lock (this)
                {
                    return images.Count - 1;
                }
            }
        }

        public ScannedImageSource AsSource() => new SinkSource(this);

        public void SetCompleted()
        {
            lock (this)
            {
                if (Completed)
                {
                    throw new InvalidOperationException("Sink is already in the completed state");
                }
                Completed = true;
                images.Last().SetResult(null);
            }
        }

        public void SetError(Exception ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }
            lock (this)
            {
                if (Completed)
                {
                    throw new InvalidOperationException("Sink is already in the completed state");
                }
                Completed = true;
                ex.PreserveStackTrace();
                images.Last().SetException(ex);
            }
        }

        public virtual void PutImage(ScannedImage image)
        {
            lock (this)
            {
                var last = images.Last();
                // Despite the lock, images.Add needs to happen before SetResult to avoid a race condition.
                // Otherwise, SetResult synchronously continues the Next() method, and user code will run.
                // Then if Next() is called again, it will be able to get the lock because it's actually on the same
                // thread that holds the lock in PutImage!
                // Yet another "gotcha" of async/await.
                images.Add(new TaskCompletionSource<ScannedImage>());
                Task.Run(() => last.SetResult(image));
            }
        }

        private class SinkSource : ScannedImageSource
        {
            private readonly ScannedImageSink sink;
            private int imagesRead;

            public SinkSource(ScannedImageSink sink)
            {
                this.sink = sink;
            }

            public override async Task<ScannedImage> Next()
            {
                TaskCompletionSource<ScannedImage> tcs;
                lock (sink)
                {
                    if (imagesRead >= sink.images.Count)
                    {
                        imagesRead--;
                    }
                    tcs = sink.images[imagesRead];
                }
                var result = await tcs.Task;
                imagesRead++;
                return result;
            }
        }
    }
}
