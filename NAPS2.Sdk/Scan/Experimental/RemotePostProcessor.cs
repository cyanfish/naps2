using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images;
using NAPS2.Images.Storage;

namespace NAPS2.Scan.Experimental
{
    public class RemotePostProcessor : IRemotePostProcessor
    {
        public (ScannedImage, PostProcessingContext) PostProcess(IImage image) => throw new NotImplementedException();
    }
}