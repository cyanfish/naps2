using System;
using System.Collections.Generic;
using System.Linq;
using WIA;

namespace NAPS2.Scan.Wia
{
    public class ConsoleWiaTransfer : IWiaTransfer
    {
        public ImageFile Transfer(int pageNumber, Item item, string format)
        {
            // The console shouldn't spawn new forms, so use the silent transfer method.
            // TODO: Test cancellation (via Ctrl+C or similar)
            return (ImageFile)item.Transfer(format);
        }
    }
}