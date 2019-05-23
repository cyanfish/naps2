using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.ImportExport
{
    public class PrintDocumentPrinter : IScannedImagePrinter
    {
        private readonly ScannedImageRenderer scannedImageRenderer;

        public PrintDocumentPrinter(ScannedImageRenderer scannedImageRenderer)
        {
            this.scannedImageRenderer = scannedImageRenderer;
        }

        public async Task<bool> PromptToPrint(List<ScannedImage> images, List<ScannedImage> selectedImages)
        {
            if (!images.Any())
            {
                return false;
            }
            var printDialog = new PrintDialog
            {
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

        public async Task<bool> Print(PrinterSettings printerSettings, List<ScannedImage> images, List<ScannedImage> selectedImages)
        {
            List<ScannedImage> imagesToPrint;
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
                    imagesToPrint = new List<ScannedImage>();
                    break;
            }
            if (imagesToPrint.Count == 0)
            {
                return false;
            }

            var snapshots = imagesToPrint.Select(x => x.Preserve()).ToList();
            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    var printDocument = new PrintDocument();
                    int i = 0;
                    printDocument.PrintPage += (sender, e) =>
                    {
                        var image = Task.Factory.StartNew(() => scannedImageRenderer.Render(imagesToPrint[i])).Unwrap().Result;
                        try
                        {
                            var pb = e.PageBounds;
                            if (Math.Sign(image.Width - image.Height) != Math.Sign(pb.Width - pb.Height))
                            {
                                // Flip portrait/landscape to match output
                                image = new RotationTransform(90).Perform(image);
                            }

                            // Fit the image into the output rect while maintaining its aspect ratio
                            var rect = image.Width / pb.Width < image.Height / pb.Height
                                ? new Rectangle(pb.Left, pb.Top, image.Width * pb.Height / image.Height, pb.Height)
                                : new Rectangle(pb.Left, pb.Top, pb.Width, image.Height * pb.Width / image.Width);

                            e.Graphics.DrawImage(image, rect);
                        }
                        finally
                        {
                            image.Dispose();
                        }

                        e.HasMorePages = (++i < imagesToPrint.Count);
                    };
                    printDocument.PrinterSettings = printerSettings;
                    printDocument.Print();
                    
                    Log.Event(EventType.Print, new Event
                    {
                        Name = MiscResources.Print,
                        Pages = snapshots.Count,
                        DeviceName = printDocument.PrinterSettings.PrinterName
                    });

                    return true;
                }
                finally
                {
                    snapshots.ForEach(s => s.Dispose());
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
