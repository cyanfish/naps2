using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaItemBase : NativeWiaObject, IWiaProps
    {
        private WiaPropertyCollection properties;

        protected internal WiaItemBase(IntPtr handle) : base(handle)
        {
        }

        public WiaPropertyCollection Properties
        {
            get
            {
                if (properties == null)
                {
                    WiaException.Check(NativeWiaMethods.GetItemPropertyStorage(Handle, out var propStorage));
                    properties = new WiaPropertyCollection(propStorage);
                }
                return properties;
            }
        }

        public IEnumerable<WiaItem> GetSubItems()
        {
            return null;
        }

        public WiaItem FindSubItem(string name)
        {
            WiaException.Check(NativeWiaMethods.GetItem(Handle, name, out var itemHandle));
            return new WiaItem(itemHandle);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                properties?.Dispose();
            }
        }
    }
}