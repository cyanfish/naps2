using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaProperty
    {
        protected internal WiaProperty(IntPtr storage, int id, string name, ushort type)
        {
            Storage = storage;
            Id = id;
            Name = name;
            Type = type;
        }

        private IntPtr Storage { get; }

        public int Id { get; set; }

        public string Name { get; }

        public ushort Type { get; }
        
        public object Value
        {
            get
            {
                if (Type == WiaPropertyType.I4)
                {
                    WiaException.Check(NativeWiaMethods.GetPropertyInt(Storage, Id, out int value));
                    return value;
                }
                if (Type == WiaPropertyType.BSTR)
                {
                    WiaException.Check(NativeWiaMethods.GetPropertyBstr(Storage, Id, out string value));
                    return value;
                }
                throw new NotImplementedException($"Not implemented property type: {Type}");
            }
            set
            {
                if (Type == WiaPropertyType.I4)
                {
                    WiaException.Check(NativeWiaMethods.SetPropertyInt(Storage, Id, (int)value));
                }
                else
                {
                    throw new NotImplementedException($"Not implemented property type: {Type}");
                }
            }
        }

        // TODO: Load attributes. Maybe inside a WiaPropertyAttributes class, Attributes property?
        // https://docs.microsoft.com/en-us/windows/desktop/api/wia_xp/nf-wia_xp-iwiapropertystorage-getpropertyattributes

        public SubTypes SubType { get; set; }

        public int SubTypeMax { get; set; }

        public int SubTypeMin { get; set; }

        public int SubTypeStep { get; set; }

        public object[] SubTypeValues { get; set; }

        public override string ToString() => Name;

        public enum SubTypes
        {
            None,
            Range,
            List,
            Flag
        }
    }
}