using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaPropertyAttributes
    {
        public WiaPropertyAttributes(IntPtr storage, int id)
        {
            // TODO
            // https://docs.microsoft.com/en-us/windows/desktop/api/wia_xp/nf-wia_xp-iwiapropertystorage-getpropertyattributes
        }

        public WiaPropertySubType SubType { get; }

        public int Max { get; }

        public int Min { get; }

        public int Step { get; }

        public object[] Values { get; }

    }
}