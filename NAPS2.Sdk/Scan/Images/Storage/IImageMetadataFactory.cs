using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public interface IImageMetadataFactory
    {
        IImageMetadata CreateMetadata(IStorage storage);
    }
}
