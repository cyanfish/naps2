using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WIA;

namespace NAPS2.Scan.Wia
{
    public class BackgroundWiaTransfer : IWiaTransfer
    {
        public Stream Transfer(int pageNumber, WiaBackgroundEventLoop eventLoop, string format)
        {
            // The console shouldn't spawn new forms, so use the silent transfer method.
            ImageFile imageFile = eventLoop.GetSync(wia => (ImageFile)wia.Item.Transfer(format));
            if (imageFile == null)
            {
                return null;
            }
            return new MemoryStream((byte[])imageFile.FileData.get_BinaryData());
        }
    }
}