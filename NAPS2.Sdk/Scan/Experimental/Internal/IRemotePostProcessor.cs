using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images;
using NAPS2.Images.Storage;

namespace NAPS2.Scan.Experimental.Internal
{
    /// <summary>
    /// Performs post-processing on an image after scanning. If a network-based IScanAdapter is used, this happens on the remote instance.
    /// </summary>
    internal interface IRemotePostProcessor
    {
        ScannedImage PostProcess(IImage image, ScanOptions options, PostProcessingContext postProcessingContext);
    }
}
