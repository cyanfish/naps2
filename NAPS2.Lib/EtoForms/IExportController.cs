namespace NAPS2.EtoForms;

public interface IExportController
{
    Task<bool> SavePdf(ICollection<UiImage> uiImages, ISaveNotify notify);
    Task<bool> SaveImages(ICollection<UiImage> uiImages, ISaveNotify notify);
    Task<bool> SavePdfOrImages(ICollection<UiImage> uiImages, ISaveNotify notify);
    Task<bool> EmailPdf(ICollection<UiImage> uiImages);
}