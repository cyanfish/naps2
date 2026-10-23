using NAPS2.Windows.Printing;

namespace NAPS2.ImportExport;

/// <summary>
/// Prints with the Windows print dialog using NAPS2.Windows.Printing (which supports print preview, unlike the
/// actual WPF PrintDialog).
/// </summary>
public class WpfScannedImagePrinter : IScannedImagePrinter
{
    public async Task<bool> PromptToPrint(
        Eto.Forms.Window parentWindow, IList<ProcessedImage> images, IList<ProcessedImage> selectedImages)
    {
        if (!images.Any())
        {
            return false;
        }
        if (!WindowsPrintDialog.IsSupported)
        {
            Log.Debug("Print dialog is not supported");
            return false;
        }
        var selectedPages = selectedImages.Select(images.IndexOf).Where(index => index >= 0).ToList();
        // Usually one page is selected, so only print the selection by default if it's more than that
        var initialPages = selectedPages.Count > 1 && selectedPages.Count < images.Count ? selectedPages : null;
        var options = new PrintDialogOptions
        {
            StartPage = selectedPages.DefaultIfEmpty(0).Min(),
            InitialPages = initialPages,
            // Pages are rotated to fit the paper, but the paper starts with the orientation of the first page
            Orientation = GetOrientation(images[initialPages?.Min() ?? 0]),
            DisplayedOptions =
            [
                PrintOption.Copies, PrintOption.Orientation, PrintOption.ColorMode, PrintOption.PageRange,
                PrintOption.PaperSize
            ]
        };
        var result = await WindowsPrintDialog.ShowAsync(parentWindow.NativeHandle, parentWindow.Title, images.Count,
            index => RenderPage(images[index]), options);
        if (!result.Printed)
        {
            return false;
        }

        Log.Event(EventType.Print, new EventParams
        {
            Name = MiscResources.Print,
            Pages = result.PageCount,
            DeviceName = result.PrinterName
        });
        return true;
    }

    private static PrintOrientation GetOrientation(ProcessedImage image)
    {
        using var rendered = image.Render();
        return rendered.Width > rendered.Height ? PrintOrientation.Landscape : PrintOrientation.Portrait;
    }

    private static PageImage RenderPage(ProcessedImage image)
    {
        using var rendered = image.Render();
        var bgra = rendered.CopyWithPixelFormat(ImagePixelFormat.ARGB32);
        var lockState = bgra.Lock(LockMode.ReadOnly, out var data);
        return new PageImage(data.w, data.h, data.stride, data.safePtr, () =>
        {
            lockState.Dispose();
            bgra.Dispose();
        });
    }
}