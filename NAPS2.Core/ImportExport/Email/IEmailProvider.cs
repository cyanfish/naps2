using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.ImportExport.Email
{
    public interface IEmailProvider
    {
        bool SendEmail(EmailMessage emailMessage);
    }
}
