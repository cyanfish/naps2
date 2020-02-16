using System;
using Google.Protobuf;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Operation;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Serialization;

namespace NAPS2.ImportExport
{
    public class DirectImportOperation : OperationBase
    {
        private readonly ImageContext _imageContext;
        private readonly ImageRenderer _imageRenderer;

        public DirectImportOperation(ImageContext imageContext, ImageRenderer imageRenderer)
        {
            _imageContext = imageContext;
            _imageRenderer = imageRenderer;

            AllowCancel = true;
            AllowBackground = true;
        }

        public bool Start(DirectImageTransfer data, bool copy, Action<ScannedImage> imageCallback, DirectImportParams importParams)
        {
            ProgressTitle = copy ? MiscResources.CopyProgress : MiscResources.ImportProgress;
            Status = new OperationStatus
            {
                StatusText = copy ? MiscResources.Copying : MiscResources.Importing,
                MaxProgress = data.SerializedImages.Count
            };

            RunAsync(async () =>
            {
                Exception? error = null;
                foreach (var serializedImageBytes in data.SerializedImages)
                {
                    try
                    {
                        var serializedImage = new SerializedImage();
                        serializedImage.MergeFrom(serializedImageBytes);
                        ScannedImage img = SerializedImageHelper.Deserialize(_imageContext, serializedImage, new SerializedImageHelper.DeserializeOptions());
                        // TODO: Don't bother, here, in recovery, etc.
                        if (importParams.ThumbnailSize.HasValue)
                        {
                            img.SetThumbnail(_imageContext.PerformTransform(await _imageRenderer.Render(img), new ThumbnailTransform(importParams.ThumbnailSize.Value)));
                        }
                        imageCallback(img);

                        Status.CurrentProgress++;
                        InvokeStatusChanged();
                        if (CancelToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                }
                if (error != null)
                {
                    Log.ErrorException(string.Format(MiscResources.ImportErrorCouldNot, "<data>"), error);
                }
                return true;
            });
            return true;
        }
    }
}
