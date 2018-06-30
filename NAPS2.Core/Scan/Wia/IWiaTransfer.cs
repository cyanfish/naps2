using System.IO;

namespace NAPS2.Scan.Wia
{
    public interface IWiaTransfer
    {
        Stream Transfer(int pageNumber, WiaBackgroundEventLoop eventLoop, string format);
    }
}