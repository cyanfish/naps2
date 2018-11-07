using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaException : Exception
    {
        public static void Check(uint hresult)
        {
            if (hresult != 0)
            {
                throw new WiaException(hresult);
            }
        }

        public WiaException(uint errorCode)
        {
            ErrorCode = errorCode;
        }

        public uint ErrorCode { get; set; }
    }
}
