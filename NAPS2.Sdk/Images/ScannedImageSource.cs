using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Util;

namespace NAPS2.Images
{
    public abstract class ScannedImageSource
    {
        public abstract Task<ScannedImage> Next();

        // TODO: Maybe make everything else an extension method?
        // TODO: I wonder if it's possible to match this signature to IAsyncEnumerable so it can be used as-is...
        // TODO: This should also have Dispose() that cleans up any further images that have not been processed (?).

        // TODO: Perhaps this should simply function as a proper IAsyncEnumerable. Have a cached list. Enumerators have an index to the list. A monitor/wait system.
        // TODO: One issue, though: How to handle exceptions?

        public async Task<List<ScannedImage>> ToList()
        {
            var list = new List<ScannedImage>();
            try
            {
                await ForEach(image => list.Add(image));
            }
            catch (Exception)
            {
                foreach (var image in list)
                {
                    image.Dispose();
                }
                throw;
            }
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

        public async Task ForEach(Func<ScannedImage, Task> action)
        {
            ScannedImage image;
            while ((image = await Next()) != null)
            {
                await action(image);
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

        // TODO: Make this a bit cleaner. Perhaps something more akin to the CancellationTokenSource/CancellationToken pattern.
        // TODO: Keep implementations internal as needed, perhaps with static factory methods?
        public class Concrete : ScannedImageSource
        {
            private readonly BlockingCollection<ScannedImage> collection = new BlockingCollection<ScannedImage>();
            private readonly CancellationTokenSource cts = new CancellationTokenSource();

            private Exception exception;

            public virtual void Put(ScannedImage image)
            {
                collection.Add(image);
                OnPut?.Invoke(this, EventArgs.Empty);
            }

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

            public event EventHandler OnPut;
        }
    }
}
