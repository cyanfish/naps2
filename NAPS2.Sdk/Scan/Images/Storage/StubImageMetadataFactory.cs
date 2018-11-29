using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public class StubImageMetadataFactory : IImageMetadataFactory
    {
        public IImageMetadata CreateMetadata(IStorage storage) => new StubImageMetadata();
    }
}
