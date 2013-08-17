using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Email.Exceptions
{
    public class MapiException : Exception
    {
        public MapiException(int returnCode)
            : base(string.Format("MAPI Error Code: {0}", returnCode))
        {
        }
    }
}
