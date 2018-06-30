namespace NAPS2.ImportExport.Email.Mapi
{
    // Documented at:
    // http://msdn.microsoft.com/en-us/library/windows/desktop/hh707275%28v=vs.85%29.aspx#MAPI_FORCE_UNICODE
    internal enum MapiSendMailReturnCode
    {
        Success = 0,
        UserAbort = 1,
        Failure = 2,
        LoginFailure = 3,
        InsufficientMemory = 5,
        TooManyFiles = 9,
        TooManyRecipients = 10,
        AttachmentNotFound = 11,
        AttachmentOpenFailure = 12,
        UnknownRecipient = 14,
        BadRecipType = 15,
        TextTooLarge = 18,
        AmbiguousRecipient = 21,
        InvalidRecips = 25,
        UnicodeNotSupported = 27
    }
}