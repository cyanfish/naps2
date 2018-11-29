using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Images.Transforms;

namespace NAPS2.Scan.Images.Storage
{
    public interface ITransformer<TStorage, in TTransform> where TStorage : IMemoryStorage where TTransform : Transform
    {
        TStorage PerformTransform(TStorage storage, TTransform transform);
    }
}
