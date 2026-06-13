namespace NAPS2.Images;

public class AutoCropOperation : OperationBase
{
    public AutoCropOperation()
    {
        AllowCancel = true;
        AllowBackground = true;
    }

    public bool Start(UiImageList imageList, List<UiImage> images, AutoCropParams autoCropParams)
    {
        ProgressTitle = MiscResources.AutoCropProgress;
        Status = new OperationStatus
        {
            StatusText = MiscResources.AutoCropping,
            MaxProgress = images.Count
        };

        RunAsync(async () =>
        {
            var beforeTransforms = new List<TransformState>();
            var afterTransforms = new List<TransformState>();
            var result = await Pipeline.For(images, CancelToken).StepParallel(img =>
            {
                using var processedImage = img.GetClonedImage();
                var image = processedImage.Render();
                try
                {
                    CancelToken.ThrowIfCancellationRequested();
                    var settings = autoCropParams.ToSettings(image.HorizontalResolution, image.VerticalResolution);
                    var transform = AutoCropper.GetCropTransform(image, settings);
                    CancelToken.ThrowIfCancellationRequested();
                    var before = img.TransformState;
                    if (transform != null)
                    {
                        var cropped = image.PerformTransform(transform);
                        var thumbnail = autoCropParams.ThumbnailSize.HasValue
                            ? cropped.PerformTransform(new ThumbnailTransform(autoCropParams.ThumbnailSize.Value))
                            : null;
                        cropped.Dispose();
                        img.AddTransform(transform, thumbnail);
                    }
                    // If transform is null no content was detected (e.g. a blank page), so
                    // the image is left unchanged. We still record before/after (which are
                    // then equal) to keep the undo element aligned with the image list.
                    var after = img.TransformState;
                    lock (this)
                    {
                        Status.CurrentProgress += 1;
                    }
                    InvokeStatusChanged();
                    return (before, after);
                }
                finally
                {
                    image.Dispose();
                }
            }).Run(transformState =>
            {
                beforeTransforms.Add(transformState.before);
                afterTransforms.Add(transformState.after);
            });
            imageList.PushUndoElement(new TransformImagesUndoElement(images, beforeTransforms, afterTransforms));
            return result;
        });

        return true;
    }
}
