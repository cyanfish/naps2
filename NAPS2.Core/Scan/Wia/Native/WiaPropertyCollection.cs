using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NAPS2.Util;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaPropertyCollection
    {
        private readonly Dictionary<int, WiaProperty> propertyDict;

        public WiaPropertyCollection(IntPtr propertyStorageHandle)
        {
            propertyDict = new Dictionary<int, WiaProperty>();
            // TODO
            try
            {
            }
            finally
            {
                Marshal.Release(propertyStorageHandle);
            }
        }

        public WiaProperty this[int propId] => propertyDict.Get(propId);
    }
}
