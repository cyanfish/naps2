using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images;

namespace NAPS2.Scan.Experimental.Internal
{
    /// <summary>
    /// Performs local post-processing on an image just before it is returned from IScanController.
    /// </summary>
    internal interface ILocalPostProcessor
    {
        void PostProcess(ScannedImage image, PostProcessingContext postProcessingContext);
    }
}
