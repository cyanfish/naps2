using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.ImportExport.Email.Mapi
{
    /// <summary>
    /// MAPI constants indicating the sender or recipient type.
    /// Documented at: http://msdn.microsoft.com/en-us/library/windows/desktop/dd296720%28v=vs.85%29.aspx
    /// </summary>
    internal enum MapiRecipClass
    {
        Sender = 0,
        To = 1,
        Cc = 2,
        Bcc = 3
    }
}
