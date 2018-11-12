using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaPropertyCollection : NativeWiaObject, IEnumerable<WiaProperty>
    {
        private readonly Dictionary<int, WiaProperty> propertyDict;

        public WiaPropertyCollection(WiaVersion version, IntPtr propertyStorageHandle) : base(version, propertyStorageHandle)
        {
            propertyDict = new Dictionary<int, WiaProperty>();
            WiaException.Check(NativeWiaMethods.EnumerateProperties(Handle,
                (id, name, type) => propertyDict.Add(id, new WiaProperty(Handle, id, name, type))));
        }
        
        public WiaProperty this[int propId] => propertyDict.Get(propId);

        public IEnumerator<WiaProperty> GetEnumerator() => propertyDict.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
