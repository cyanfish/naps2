using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.ImportExport.Email.Mapi
{
    // Documented at:
    // http://msdn.microsoft.com/en-us/library/windows/desktop/hh707275%28v=vs.85%29.aspx#MAPI_FORCE_UNICODE
    public enum MapiSendMailReturnCode
    {
        Success = 0,
        UserAbort = 1,
        Failure = 2,

        AmbiguousRecipient = 21,
        AttachmentNotFound = 11,
        AttachmentOpenFailure = 12,
        BadRecipType = 15,
        InsufficientMemory = 5,
        InvalidRecips = 25,
        LoginFailure = 3,
        TextTooLarge = 18,
        TooManyFiles = 9,
        TooManyRecipients = 10,
        UnicodeNotSupported = 27,
        UnknownRecipient = 14
    }
}
