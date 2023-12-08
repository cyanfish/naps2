using Gtk;
using NAPS2.Images.Gtk;

namespace NAPS2.ImportExport;

public class GtkScannedImagePrinter : IScannedImagePrinter
{
    public Task<bool> PromptToPrint(
        Eto.Forms.Window parentWindow, IList<ProcessedImage> images, IList<ProcessedImage> selectedImages)
    {
        if (!images.Any())
        {
            return Task.FromResult(false);
        }
        var printOp = new PrintOperation
        {
            NPages = images.Count,
            UseFullPage = true,
            HasSelection = selectedImages.Count > 1,
            SupportSelection = selectedImages.Count > 1
        };
        if (selectedImages.Count == 1)
        {
            printOp.CurrentPage = images.IndexOf(selectedImages[0]);
        }
        var printTarget = images;
        printOp.BeginPrint += (_, args) =>
        {
            if (printOp.PrintSettings.PrintPages == PrintPages.Selection)
            {
                printTarget = selectedImages;
                printOp.NPages = printTarget.Count;
            }
        };
        printOp.DrawPage += (_, args) =>
        {
            var image = printTarget[args.PageNr].Render();
            try
            {
                var ctx = args.Context;
                var cairoCtx = ctx.CairoContext;

                if (Math.Sign(image.Width - image.Height) != Math.Sign(ctx.Width - ctx.Height))
                {
                    // Flip portrait/landscape to match output
                    image = image.PerformTransform(new RotationTransform(90));
                }

                // Fit the image into the output rect (centered) while maintaining its aspect ratio
                var heightBound = image.Width / ctx.Width < image.Height / ctx.Height;
                var targetWidth = heightBound ? image.Width * ctx.Height / image.Height : ctx.Width;
                var targetHeight = heightBound ? ctx.Height : image.Height * ctx.Width / image.Width;
                var targetX = (ctx.Width - targetWidth) / 2;
                var targetY = (ctx.Height - targetHeight) / 2;
                cairoCtx.Translate(targetX, targetY);
                cairoCtx.Scale(targetWidth / image.Width, targetHeight / image.Height);

                Gdk.CairoHelper.SetSourcePixbuf(cairoCtx, image.AsPixbuf(), 0, 0);
                cairoCtx.Paint();
            }
            finally
            {
                image.Dispose();
            }
        };
        printOp.EndPrint += (_, args) =>
        {
            Log.Event(EventType.Print, new EventParams
            {
                Name = MiscResources.Print,
                Pages = printOp.NPagesToPrint
            });
        };
        var result = printOp.Run(PrintOperationAction.PrintDialog, (Window) parentWindow.ControlObject);
        return Task.FromResult(result == PrintOperationResult.Apply);
    }
}