using NAPS2.ImportExport.Email;

namespace NAPS2.WinForms;

public interface IWinFormsExportHelper
{
    Task<bool> SavePDF(IList<ProcessedImage> images, ISaveNotify notify);
    Task<bool> ExportPDF(string filename, IList<ProcessedImage> images, bool email, EmailMessage emailMessage);
    Task<bool> SaveImages(IList<ProcessedImage> images, ISaveNotify notify);
    Task<bool> EmailPDF(IList<ProcessedImage> images);
}