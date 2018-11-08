using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaPropertyCollection : NativeWiaObject
    {
        private readonly Dictionary<int, WiaProperty> propertyDict;

        public WiaPropertyCollection(IntPtr propertyStorageHandle) : base(propertyStorageHandle)
        {
            propertyDict = new Dictionary<int, WiaProperty>();
            WiaException.Check(NativeWiaMethods.EnumerateProperties(Handle,
                (id, name, type) => propertyDict.Add(id, new WiaProperty(Handle, id, name, type))));
        }
        
        public WiaProperty this[int propId] => propertyDict.Get(propId);
    }
}
