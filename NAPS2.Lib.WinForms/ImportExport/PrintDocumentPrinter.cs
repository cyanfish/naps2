using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using NAPS2.Images.Gdi;

namespace NAPS2.ImportExport;

public class PrintDocumentPrinter : IScannedImagePrinter
{
    public async Task<bool> PromptToPrint(
        Eto.Forms.Window parentWindow, IList<ProcessedImage> images, IList<ProcessedImage> selectedImages)
    {
        if (!images.Any())
        {
            return false;
        }
        var printDialog = new PrintDialog
        {
            // TODO: If we migrate this to WPF we might be able to enable print previews
            // WinForms has UseEXDialog which will use the modern windows dialog but has no way to populate the preview
            AllowSelection = selectedImages.Any(),
            AllowSomePages = true,
            PrinterSettings =
            {
                MinimumPage = 1,
                MaximumPage = images.Count,
                FromPage = 1,
                ToPage = images.Count
            }
        };
        if (printDialog.ShowDialog() == DialogResult.OK)
        {
            return await Print(printDialog.PrinterSettings, images, selectedImages);
        }
        return false;
    }

    public async Task<bool> Print(PrinterSettings printerSettings, IList<ProcessedImage> images,
        IList<ProcessedImage> selectedImages)
    {
        IList<ProcessedImage> imagesToPrint;
        switch (printerSettings.PrintRange)
        {
            case PrintRange.AllPages:
                imagesToPrint = images;
                break;
            case PrintRange.Selection:
                imagesToPrint = selectedImages;
                break;
            case PrintRange.SomePages:
                int start = printerSettings.FromPage - 1;
                int length = printerSettings.ToPage - start;
                imagesToPrint = images.Skip(start).Take(length).ToList();
                break;
            default:
                imagesToPrint = [];
                break;
        }
        if (imagesToPrint.Count == 0)
        {
            return false;
        }

        return await Task.Run(() =>
        {
            var printDocument = new PrintDocument();
            int i = 0;
            printDocument.PrintPage += (sender, e) =>
            {
                var image = imagesToPrint[i].Render();
                try
                {
                    var pb = e.PageBounds;
                    if (Math.Sign(image.Width - image.Height) != Math.Sign(pb.Width - pb.Height))
                    {
                        // Flip portrait/landscape to match output
                        image = image.PerformTransform(new RotationTransform(90));
                    }

                    // Fit the image into the output rect while maintaining its aspect ratio
                    var rect = image.Width / pb.Width < image.Height / pb.Height
                        ? new Rectangle(pb.Left, pb.Top, image.Width * pb.Height / image.Height, pb.Height)
                        : new Rectangle(pb.Left, pb.Top, pb.Width, image.Height * pb.Width / image.Width);

                    e.Graphics!.DrawImage(image.AsBitmap(), rect);
                }
                finally
                {
                    image.Dispose();
                }

                e.HasMorePages = (++i < imagesToPrint.Count);
            };
            printDocument.PrinterSettings = printerSettings;
            printDocument.Print();

            Log.Event(EventType.Print, new EventParams
            {
                Name = MiscResources.Print,
                Pages = images.Count,
                DeviceName = printDocument.PrinterSettings.PrinterName
            });

            return true;
        });
    }
}