using Xunit;

namespace NAPS2.Sdk.Tests;

public abstract class StorageConfig
{
    public abstract void Apply(ContextualTests contextualTests);
    
    public class Memory : StorageConfig
    {
        public override void Apply(ContextualTests contextualTests)
        {
        }

        public override void AssertPdfStorage(IImageStorage storage)
        {
            var memoryStorage = Assert.IsType<ImageMemoryStorage>(storage);
            Assert.Equal(".pdf", memoryStorage.TypeHint);
        }

        public override void AssertJpegStorage(IImageStorage storage)
        {
            Assert.IsAssignableFrom<IMemoryImage>(storage);
        }

        public override void AssertPngStorage(IImageStorage storage)
        {
            Assert.IsAssignableFrom<IMemoryImage>(storage);
        }
    }

    public class File : StorageConfig
    {
        public override void Apply(ContextualTests ctx)
        {
            ctx.SetUpFileStorage();
        }

        public override void AssertPdfStorage(IImageStorage storage)
        {
            var fileStorage = Assert.IsType<ImageFileStorage>(storage);
            Assert.Equal(".pdf", Path.GetExtension(fileStorage.FullPath));
        }

        public override void AssertJpegStorage(IImageStorage storage)
        {
            var fileStorage = Assert.IsType<ImageFileStorage>(storage);
            Assert.Equal(".jpg", Path.GetExtension(fileStorage.FullPath));
        }

        public override void AssertPngStorage(IImageStorage storage)
        {
            var fileStorage = Assert.IsType<ImageFileStorage>(storage);
            Assert.Equal(".png", Path.GetExtension(fileStorage.FullPath));
        }
    }

    public abstract void AssertPdfStorage(IImageStorage storage);
    public abstract void AssertJpegStorage(IImageStorage storage);
    public abstract void AssertPngStorage(IImageStorage storage);
}