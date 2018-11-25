using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.ImportExport.Email
{
    public interface IEmailProviderFactory
    {
        IEmailProvider Create(EmailProviderType type);

        IEmailProvider Default { get; }
    }
}
