using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.ImportExport.Email.Mapi
{
    // Documented at:
    // http://msdn.microsoft.com/en-us/library/windows/desktop/hh707275%28v=vs.85%29.aspx#MAPI_DIALOG
    [Flags]
    internal enum MapiSendMailFlags
    {
        None = 0,
        LogonUI = 1,
        Dialog = 8
    }
}
