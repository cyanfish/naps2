using System.Threading;

namespace NAPS2.ImportExport.Email;

public interface IEmailProvider
{
    Task<bool> SendEmail(EmailMessage emailMessage, ProgressHandler progressCallback, CancellationToken cancelToken);
}