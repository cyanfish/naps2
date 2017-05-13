using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.ImportExport.Email
{
    public interface IEmailer
    {
        bool SendEmail(EmailMessage emailMessage);
    }
}
