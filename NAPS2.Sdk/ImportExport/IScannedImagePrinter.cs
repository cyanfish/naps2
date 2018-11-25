using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Scan.Images;

namespace NAPS2.ImportExport
{
    public interface IScannedImagePrinter
    {
        /// <summary>
        /// Prints the provided images, prompting the user for the printer settings.
        /// </summary>
        /// <param name="images">The full list of images to print.</param>
        /// <param name="selectedImages">The list of selected images. If non-empty, the user will be presented an option to print selected.</param>
        /// <returns>True if the print completed, false if there was nothing to print or the user cancelled.</returns>
        Task<bool> PromptToPrint(List<ScannedImage> images, List<ScannedImage> selectedImages);

        /// <summary>
        /// Prints the provided images with the specified printer settings.
        /// </summary>
        /// <param name="printerSettings">The printer settings.</param>
        /// <param name="images">The full list of images to print.</param>
        /// <param name="selectedImages">The list of selected images, to be used if the printer settings specify to print selected.</param>
        /// <returns>True if the print completed, false if there was nothing to print.</returns>
        Task<bool> Print(PrinterSettings printerSettings, List<ScannedImage> images, List<ScannedImage> selectedImages);
    }
}
