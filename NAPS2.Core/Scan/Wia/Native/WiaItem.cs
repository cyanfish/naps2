using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaItem : NativeWiaObject
    {
        private Dictionary<int, WiaProperty> propertyDict;

        protected internal WiaItem(IntPtr handle) : base(handle)
        {
        }

        private void LoadProperties()
        {
            propertyDict = new Dictionary<int, WiaProperty>();
            // TODO
        }

        public WiaProperty Property(int propId)
        {
            if (propertyDict == null)
            {
                LoadProperties();
            }
            return propertyDict.Get(propId);
        }

        public IEnumerable<WiaItem> GetSubItems()
        {
            return null;
        }

        public WiaTransfer StartTransfer()
        {
            WiaException.Check(NativeWiaMethods.StartTransfer(Handle, out var transferHandle));
            return new WiaTransfer(transferHandle);
        }

        public WiaItem FindSubItem(string name)
        {
            WiaException.Check(NativeWiaMethods.GetItem(Handle, name, out var itemHandle));
            return new WiaItem(itemHandle);
        }
    }
}