using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public interface IFileStorage : IStorage
    {
        string FullPath { get; }
    }
}
