namespace NAPS2.ImportExport;

public interface IScannedImagePrinter
{
    /// <summary>
    /// Prints the provided images, prompting the user for the printer settings.
    /// </summary>
    /// <param name="images">The full list of images to print.</param>
    /// <param name="selectedImages">The list of selected images. If non-empty, the user will be presented an option to print selected.</param>
    /// <returns>True if the print completed, false if there was nothing to print or the user cancelled.</returns>
    Task<bool> PromptToPrint(IList<ProcessedImage> images, IList<ProcessedImage> selectedImages);
}