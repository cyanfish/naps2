using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAPS2.ImportExport.Email.Imap;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Util;

namespace NAPS2.ImportExport.Email
{
    public interface IEmailProviderFactory
    {
        IEmailProvider Create(EmailProviderType type);
    }
}
