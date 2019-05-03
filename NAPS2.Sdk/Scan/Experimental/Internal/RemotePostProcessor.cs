using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Platform;

namespace NAPS2.Scan.Experimental.Internal
{
    internal class RemotePostProcessor : IRemotePostProcessor
    {
        public (ScannedImage, PostProcessingContext) PostProcess(IImage image, ScanOptions options)
        {
            if (!PlatformCompat.System.CanUseWin32 && options.BitDepth == BitDepth.BlackAndWhite)
            {
                // TODO: Don't do this here, do it where BitmapHelper is used or something
                image = Transform.Perform(image, new BlackWhiteTransform(-options.Brightness));
            }
            throw new NotImplementedException();
        }
    }
}