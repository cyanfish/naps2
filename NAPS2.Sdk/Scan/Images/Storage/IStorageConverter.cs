using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public interface IStorageConverter<in TStorage1, out TStorage2>
    {
        TStorage2 Convert(TStorage1 input, StorageConvertParams convertParams);
    }
}
