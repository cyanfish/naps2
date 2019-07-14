using System;
using System.Collections.Generic;
using Moq;
using NAPS2.Images;
using NAPS2.Scan.Experimental;
using NAPS2.Scan.Experimental.Internal;
using NAPS2.Sdk.Tests.Mocks;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan
{
    public class ScanErrorHandling : ContextualTexts
    {
        [Fact]
        public void InvalidOptions()
        {
            var localPostProcessor = new Mock<ILocalPostProcessor>();
            var bridgeFactory = new Mock<IScanBridgeFactory>();
            var controller = new ScanController(localPostProcessor.Object, new ScanOptionsValidator(), bridgeFactory.Object);

            var invalidOptions = new ScanOptions { Dpi = -1 };
            Assert.Throws<ArgumentException>(() => controller.Scan(invalidOptions));
        }

        [Fact]
        public async void CreateScanBridge()
        {
            var localPostProcessor = new Mock<ILocalPostProcessor>();
            var bridgeFactory = new Mock<IScanBridgeFactory>();
            var controller = new ScanController(localPostProcessor.Object, new ScanOptionsValidator(), bridgeFactory.Object);
            
            bridgeFactory.Setup(factory => factory.Create(It.IsAny<ScanOptions>())).Throws<InvalidOperationException>();
            var source = controller.Scan(new ScanOptions());
            await Assert.ThrowsAsync<InvalidOperationException>(source.ToList);
        }

        [Fact]
        public async void LocalPostProcess()
        {
            var localPostProcessor = new Mock<ILocalPostProcessor>();
            var bridge = new StubScanBridge { MockOutput = new List<ScannedImage> { CreateScannedImage() } };
            var bridgeFactory = new Mock<IScanBridgeFactory>();
            var controller = new ScanController(localPostProcessor.Object, new ScanOptionsValidator(), bridgeFactory.Object);
            
            bridgeFactory.Setup(factory => factory.Create(It.IsAny<ScanOptions>())).Returns(bridge);
            localPostProcessor.Setup(pp => pp.PostProcess(It.IsAny<ScannedImage>(), It.IsAny<ScanOptions>(), It.IsAny<PostProcessingContext>()))
                .Throws<InvalidOperationException>();
            var source = controller.Scan(new ScanOptions());
            await Assert.ThrowsAsync<InvalidOperationException>(source.ToList);
        }
    }
}
