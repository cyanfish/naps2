using System;

namespace NAPS.Email
{
    public interface IEmailer
    {
        bool SendEmail(string attachmentFileName, string subject);
    }
}
