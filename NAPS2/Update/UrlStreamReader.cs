using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NAPS2.Update
{
    public class UrlStreamReader : IUrlStreamReader
    {
        public Stream OpenStream(string url)
        {
            var req = WebRequest.Create(url);
            return req.GetResponse().GetResponseStream();
        }
    }
}
