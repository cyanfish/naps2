using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WIA;

namespace NAPS2.Scan.Wia
{
    public interface IWiaTransfer
    {
        ImageFile Transfer(int pageNumber, Device device, Item item, string format);
    }
}
