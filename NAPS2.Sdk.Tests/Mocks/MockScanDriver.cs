using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.ImportExport;
using NAPS2.Scan;
using NAPS2.Util;

namespace NAPS2.Sdk.Tests.Mocks
{
    public class MockScanDriver : ScanDriverBase
    {
        public MockScanDriver(ErrorOutput errorOutput, AutoSaver autoSaver) : base(errorOutput, autoSaver)
        {
        }

        public string MockDriverName { get; set; } = "mock";

        public List<ScanDevice> MockDevices { get; set; } = new List<ScanDevice>();

        public List<ScannedImage> MockOutput { get; set; } = new List<ScannedImage>();

        public Exception MockError { get; set; }

        public override string DriverName => "test";

        public override bool IsSupported => true;

        protected override List<ScanDevice> GetDeviceListInternal(ScanProfile scanProfile) => MockDevices;

        protected override Task ScanInternal(ScannedImageSink sink, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent, CancellationToken cancelToken)
        {
            foreach (var img in MockOutput)
            {
                sink.PutImage(img);
            }
            if (MockError != null)
            {
                throw MockError;
            }
            return Task.CompletedTask;
        }
    }
}
