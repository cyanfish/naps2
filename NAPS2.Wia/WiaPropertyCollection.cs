using System;
using System.Collections;
using System.Collections.Generic;

namespace NAPS2.Wia
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
        
        public WiaProperty this[int propId] => propertyDict.ContainsKey(propId) ? propertyDict[propId] : null;

        public IEnumerator<WiaProperty> GetEnumerator() => propertyDict.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
