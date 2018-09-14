using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Util;

namespace NAPS2.Scan.Images
{
    public abstract class ScannedImageSource
    {
        public abstract Task<ScannedImage> Next();

        public async Task<List<ScannedImage>> ToList()
        {
            var list = new List<ScannedImage>();
            await ForEach(image => list.Add(image));
            return list;
        }

        public async Task ForEach(Action<ScannedImage> action)
        {
            ScannedImage image;
            while ((image = await Next()) != null)
            {
                action(image);
            }
        }

        public ScannedImageSource Then(Action<ScannedImage> action)
        {
            return new ThenSource(this, action);
        }

        private class ThenSource : ScannedImageSource
        {
            private readonly ScannedImageSource inner;
            private readonly Action<ScannedImage> action;

            public ThenSource(ScannedImageSource scannedImageSource, Action<ScannedImage> action)
            {
                inner = scannedImageSource;
                this.action = action;
            }

            public override async Task<ScannedImage> Next()
            {
                var image = await inner.Next();
                if (image != null) action(image);
                return image;
            }
        }

        public class Concrete : ScannedImageSource
        {
            private readonly BlockingCollection<ScannedImage> collection = new BlockingCollection<ScannedImage>();
            private readonly CancellationTokenSource cts = new CancellationTokenSource();

            private Exception exception;

            public virtual void Put(ScannedImage image) => collection.Add(image);

            public void Done() => collection.CompleteAdding();

            public void Error(Exception ex)
            {
                exception = ex ?? throw new ArgumentNullException();
                exception.PreserveStackTrace();
                cts.Cancel();
            }

            public override Task<ScannedImage> Next() => Task.Factory.StartNew(() =>
            {
                try
                {
                    return collection.Take(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // The Error method was called
                    throw exception;
                }
                catch (InvalidOperationException)
                {
                    // The Done method was called
                    return null;
                }
            });
        }
    }
}
