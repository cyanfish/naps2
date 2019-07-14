using NAPS2.Images.Storage;

namespace NAPS2.Scan.Stub
{
    public class StubScanDriverFactory : IScanDriverFactory
    {
        private readonly ImageContext imageContext;

        public StubScanDriverFactory(ImageContext imageContext)
        {
            this.imageContext = imageContext;
        }

        public IScanDriver Create(string driverName) => new StubScanDriver(imageContext, driverName);
    }
}
