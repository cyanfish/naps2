using Eto.Forms;

namespace NAPS2.ImportExport;

public class StubScannedImagePrinter : IScannedImagePrinter
{
    public Task<bool> PromptToPrint(Window parentWindow, IList<ProcessedImage> images,
        IList<ProcessedImage> selectedImages) => Task.FromResult(false);
}
