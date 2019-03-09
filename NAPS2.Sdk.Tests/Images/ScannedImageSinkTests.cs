using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Images.Storage;
using Xunit;

namespace NAPS2.Sdk.Tests.Images
{
    public class ScannedImageSinkTests
    {
        [Fact]
        public async Task NoImages()
        {
            var sink = new ScannedImageSink();
            sink.SetCompleted();

            var source = sink.AsSource();
            Assert.Null(await source.Next());
        }

        [Fact]
        public async Task OneImage()
        {
            var sink = new ScannedImageSink();
            var img1 = CreateScannedImage();
            sink.PutImage(img1);
            sink.SetCompleted();

            var source = sink.AsSource();
            Assert.Equal(img1, await source.Next());
            Assert.Null(await source.Next());
        }

        [Fact]
        public async Task TwoImages()
        {
            var sink = new ScannedImageSink();
            var img1 = CreateScannedImage();
            var img2 = CreateScannedImage();
            sink.PutImage(img1);
            sink.PutImage(img2);
            sink.SetCompleted();

            var source = sink.AsSource();
            Assert.Equal(img1, await source.Next());
            Assert.Equal(img2, await source.Next());
            Assert.Null(await source.Next());
        }

        [Fact]
        public async Task PropagatesError()
        {
            var sink = new ScannedImageSink();
            var error = new Exception();
            sink.SetError(error);

            var source = sink.AsSource();
            await Assert.ThrowsAsync<Exception>(() => source.Next());
        }

        [Fact]
        public async Task PropagatesErrorAfterImages()
        {
            var sink = new ScannedImageSink();
            var error = new Exception();
            sink.PutImage(CreateScannedImage());
            sink.PutImage(CreateScannedImage());
            sink.SetError(error);

            var source = sink.AsSource();
            await source.Next();
            await source.Next();
            await Assert.ThrowsAsync<Exception>(() => source.Next());
        }

        private ScannedImage CreateScannedImage()
        {
            return new ScannedImage(new GdiImage(new Bitmap(100, 100)));
        }
    }
}
