using NAPS2.Images.Mac;

namespace NAPS2.ImportExport;

public class MacScannedImagePrinter : IScannedImagePrinter
{
    public Task<bool> PromptToPrint(
        Eto.Forms.Window parentWindow, IList<ProcessedImage> images, IList<ProcessedImage> selectedImages)
    {
        if (!images.Any())
        {
            return Task.FromResult(false);
        }
        using var view = new PaginatedImageView(images);
        var printOp = NSPrintOperation.FromView(view, new NSPrintInfo
        {
            LeftMargin = 0,
            BottomMargin = 0,
            RightMargin = 0,
            TopMargin = 0,
            HorizontalPagination = NSPrintingPaginationMode.Fit,
            VerticalPagination = NSPrintingPaginationMode.Fit,
            HorizontallyCentered = true,
            VerticallyCentered = true,
            Orientation = view.CurrentImage!.Width > view.CurrentImage.Height
                ? NSPrintingOrientation.Landscape
                : NSPrintingOrientation.Portrait
        });
        if (printOp.RunOperation())
        {
            Log.Event(EventType.Print, new EventParams
            {
                Name = MiscResources.Print,
                Pages = (int) printOp.PageRange.Length
            });
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    private class PaginatedImageView : NSBox
    {
        private readonly IList<ProcessedImage> _images;

        private int _pageToRender;
        private int _lastRenderedPage = -1;

        public PaginatedImageView(IList<ProcessedImage> images)
        {
            _images = images;
            // TODO: Fix deprecation issue
#pragma warning disable CA1422
            BorderType = NSBorderType.NoBorder;
#pragma warning restore CA1422
            TitlePosition = NSTitlePosition.NoTitle;
            LoadImage();
        }

        public IMemoryImage? CurrentImage { get; private set; }

        public override bool KnowsPageRange(ref NSRange range)
        {
            range = new NSRange(1, _images.Count);
            return true;
        }

        public override void BeginPage(CGRect rect, CGPoint placement)
        {
            bool loaded = LoadImage();
            if (loaded && Math.Sign(CurrentImage!.Width - CurrentImage.Height) != Math.Sign(Frame.Width - Frame.Height))
            {
                // Flip portrait/landscape to match output
                var isOriginalPortrait = CurrentImage!.Width < CurrentImage.Height;
                var angle = isOriginalPortrait ? -90 : 90;
                CurrentImage = CurrentImage.PerformTransform(new RotationTransform(angle));
            }
            ContentView = new NSImageView
            {
                Image = CurrentImage!.AsNsImage(),
                ImageAlignment = NSImageAlignment.Center,
                ImageScaling = NSImageScale.ProportionallyUpOrDown
            };
            base.BeginPage(rect, placement);
        }

        public override CGRect RectForPage(nint pageNumber)
        {
            _pageToRender = (int) pageNumber - 1;
            var operation = NSPrintOperation.CurrentOperation;
            if (Frame.Size != operation.PrintInfo.PaperSize)
            {
                SetFrameSize(operation.PrintInfo.PaperSize);
            }
            return new CGRect(new CGPoint(0, 0), operation.PrintInfo.PaperSize);
        }

        private bool LoadImage()
        {
            if (_lastRenderedPage == _pageToRender) return false;
            _lastRenderedPage = _pageToRender;
            CurrentImage?.Dispose();
            CurrentImage = _images[_pageToRender].Render();
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CurrentImage?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}