using System;
using System.Collections;
using System.Collections.Generic;

namespace NAPS2.Wia
{
    public class WiaPropertyCollection : NativeWiaObject, IEnumerable<WiaProperty>
    {
        private readonly Dictionary<int, WiaProperty> _propertyDict;

        public WiaPropertyCollection(WiaVersion version, IntPtr propertyStorageHandle) : base(version, propertyStorageHandle)
        {
            _propertyDict = new Dictionary<int, WiaProperty>();
            WiaException.Check(NativeWiaMethods.EnumerateProperties(Handle,
                (id, name, type) => _propertyDict.Add(id, new WiaProperty(Handle, id, name, type))));
        }
        
        public WiaProperty this[int propId] => _propertyDict[propId];
        
        public WiaProperty? GetOrNull(int propId) => _propertyDict.ContainsKey(propId) ? _propertyDict[propId] : null;

        public IEnumerator<WiaProperty> GetEnumerator() => _propertyDict.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
