using System;

namespace NAPS2.Wia
{
    public class WiaProperty
    {
        private WiaPropertyAttributes? _attributes;

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
                uint hr;
                if (Type == WiaPropertyType.I4)
                {
                    hr = NativeWiaMethods.SetPropertyInt(Storage, Id, (int) value);
                }
                else
                {
                    throw new NotImplementedException($"Not implemented property type: {Type}");
                }

                if (hr == Hresult.E_INVALIDARG)
                {
                    throw new ArgumentException($"Could not set property {Id} ({Name}) value to \"{value}\"", nameof(value));
                }

                WiaException.Check(hr);
            }
        }

        public WiaPropertyAttributes Attributes => _attributes ?? (_attributes = new WiaPropertyAttributes(Storage, Id));

        public override string ToString() => Name;

    }
}