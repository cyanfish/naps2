using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using NAPS2.Images.Wpf;
using Window = Eto.Forms.Window;

namespace NAPS2.ImportExport;

public class WpfScannedImagePrinter : IScannedImagePrinter
{
    public async Task<bool> PromptToPrint(
        Window parentWindow, IList<ProcessedImage> images, IList<ProcessedImage> selectedImages)
    {
        if (!images.Any())
        {
            return false;
        }
        var printDialog = new PrintDialog
        {
            SelectedPagesEnabled = selectedImages.Any(),
            UserPageRangeEnabled = true,
            MinPage = 1,
            MaxPage = (uint) images.Count,
            PageRange = new PageRange(1, images.Count)
        };
        if (printDialog.ShowDialog() != true)
        {
            return false;
        }
        var imagesToPrint = printDialog.PageRangeSelection switch
        {
            PageRangeSelection.AllPages => images,
            PageRangeSelection.SelectedPages => selectedImages,
            PageRangeSelection.UserPages => images
                .Skip(printDialog.PageRange.PageFrom - 1)
                .Take(printDialog.PageRange.PageTo - printDialog.PageRange.PageFrom + 1)
                .ToList(),
            _ => []
        };
        if (imagesToPrint.Count == 0)
        {
            return false;
        }

        var pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
        var writer = PrintQueue.CreateXpsDocumentWriter(printDialog.PrintQueue);
        var completed = new TaskCompletionSource<bool>();
        writer.WritingCompleted += (_, e) =>
        {
            if (e.Error != null) completed.SetException(e.Error);
            else completed.SetResult(!e.Cancelled);
        };
        // Pages are rendered as they're needed so they don't all have to be in memory at once
        writer.WriteAsync(new ImagePaginator(imagesToPrint, pageSize), printDialog.PrintTicket);
        bool printed = await completed.Task;

        Log.Event(EventType.Print, new EventParams
        {
            Name = MiscResources.Print,
            Pages = images.Count,
            DeviceName = printDialog.PrintQueue.FullName
        });

        return printed;
    }

    private class ImagePaginator(IList<ProcessedImage> images, Size pageSize) : DocumentPaginator
    {
        public override DocumentPage GetPage(int pageNumber)
        {
            var image = images[pageNumber].Render();
            try
            {
                if (Math.Sign(image.Width - image.Height) != Math.Sign(PageSize.Width - PageSize.Height))
                {
                    // Flip portrait/landscape to match output
                    image = image.PerformTransform(new RotationTransform(90));
                }

                // Fit the image into the page while maintaining its aspect ratio
                var rect = image.Width / PageSize.Width < image.Height / PageSize.Height
                    ? new Rect(0, 0, image.Width * PageSize.Height / image.Height, PageSize.Height)
                    : new Rect(0, 0, PageSize.Width, image.Height * PageSize.Width / image.Width);

                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen())
                {
                    dc.DrawImage(image.ToBitmapSource(), rect);
                }
                return new DocumentPage(visual, PageSize, new Rect(PageSize), rect);
            }
            finally
            {
                image.Dispose();
            }
        }

        public override bool IsPageCountValid => true;

        public override int PageCount => images.Count;

        public override Size PageSize { get; set; } = pageSize;

        public override IDocumentPaginatorSource? Source => null;
    }
}
