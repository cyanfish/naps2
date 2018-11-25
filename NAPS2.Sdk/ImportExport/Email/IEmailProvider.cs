using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Util;

namespace NAPS2.ImportExport.Email
{
    public interface IEmailProvider
    {
        Task<bool> SendEmail(EmailMessage emailMessage, ProgressHandler progressCallback, CancellationToken cancelToken);
    }
}
