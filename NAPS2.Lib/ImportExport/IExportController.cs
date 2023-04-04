using NAPS2.EtoForms.Notifications;

namespace NAPS2.ImportExport;

public interface IExportController
{
    Task<bool> SavePdf(ICollection<UiImage> uiImages, ISaveNotify notify);
    Task<bool> SaveImages(ICollection<UiImage> uiImages, ISaveNotify notify);
    Task<bool> SavePdfOrImages(ICollection<UiImage> uiImages, ISaveNotify notify);
    Task<bool> EmailPdf(ICollection<UiImage> uiImages);
}