using System.Threading.Tasks;

namespace NAPS2.ImportExport.Email.Mapi;

public interface IMapiWrapper
{
    bool CanLoadClient { get; }

    Task<MapiSendMailReturnCode> SendEmail(EmailMessage message);
}