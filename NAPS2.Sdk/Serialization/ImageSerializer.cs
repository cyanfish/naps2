using Google.Protobuf;
using NAPS2.Scan;

namespace NAPS2.Serialization;

public static class ImageSerializer
{
    public static SerializedImage Serialize(ProcessedImage image, SerializeImageOptions options)
    {
        if (options.CrossDevice && (options.RequireFileStorage || options.TransferOwnership))
        {
            throw new ArgumentException();
        }
        if (options.RequireFileStorage && image.Storage is not ImageFileStorage)
        {
            throw new ArgumentException("FileStorage is required for serialization.");
        }
        if (options.CrossDevice && options.RenderedFilePath != null)
        {
            throw new ArgumentException("A RenderedFilePath can't be specified for cross-device serialization.");
        }

        // TODO: What if there are transforms? Does it make sense to include the thumbnail from the postprocessing data
        // TODO: only, or can we somehow serialize the thumbnail from UiImage?
        MemoryStream? thumbStream = null;
        var thumb = image.PostProcessingData.Thumbnail;
        if (thumb != null && options.IncludeThumbnail &&
            image.PostProcessingData.ThumbnailTransformState == image.TransformState)
        {
            // TODO: Better format choice?
            thumbStream = thumb.SaveToMemoryStream(ImageFileFormat.Png);
        }

        var result = new SerializedImage
        {
            TransferOwnership = options.TransferOwnership,
            Metadata = new SerializedImageMetadata
            {
                TransformListXml = image.TransformState.Transforms.ToXml(),
                BitDepth = (SerializedImageMetadata.Types.BitDepth) image.Metadata.BitDepth,
                Lossless = image.Metadata.Lossless
            },
            Thumbnail = thumbStream != null ? ByteString.FromStream(thumbStream) : ByteString.Empty,
            RenderedFilePath = options.RenderedFilePath ?? ""
        };

        switch (image.Storage)
        {
            case ImageFileStorage fileStorage:
                if (options.CrossDevice)
                {
                    using var stream = File.OpenRead(fileStorage.FullPath);
                    result.FileContent = ByteString.FromStream(stream);
                    result.TypeHint = Path.GetExtension(fileStorage.FullPath).ToLowerInvariant();
                }
                else
                {
                    result.FilePath = fileStorage.FullPath;
                }
                break;
            case ImageMemoryStorage memoryStorage:
                result.FileContent = ByteString.FromStream(memoryStorage.Stream);
                result.TypeHint = memoryStorage.TypeHint;
                break;
            case IMemoryImage imageStorage:
                var fileFormat = imageStorage.OriginalFileFormat == ImageFileFormat.Unspecified
                    ? ImageFileFormat.Jpeg
                    : imageStorage.OriginalFileFormat;
                result.FileContent = ByteString.FromStream(imageStorage.SaveToMemoryStream(fileFormat));
                result.TypeHint = fileFormat.AsTypeHint();
                break;
        }

        if (options.TransferOwnership)
        {
            if (image.Storage is ImageFileStorage fileStorage)
            {
                fileStorage.MarkShared();
                image.Dispose();
                if (!fileStorage.IsDisposed)
                {
                    throw new ArgumentException(
                        "Serialization with TransferOwnership can't be used when there are multiple ProcessedImage objects referencing the same underlying storage.");
                }
            }
            else
            {
                image.Dispose();
            }
        }

        return result;
    }

    public static ProcessedImage Deserialize(ScanningContext scanningContext, SerializedImage serializedImage,
        DeserializeImageOptions options)
    {
        IImageStorage storage;
        if (!string.IsNullOrEmpty(serializedImage.FilePath))
        {
            if (serializedImage.TransferOwnership)
            {
                storage = new ImageFileStorage(serializedImage.FilePath);
            }
            else if (options.ShareFileStorage)
            {
                // TODO: Think about what exactly the contract is for the serializer and image lifetime.
                // For example, what happens when we copy an image, delete it, then try to paste?
                storage = new ImageFileStorage(serializedImage.FilePath, true);
            }
            else
            {
                // Not transfering or sharing the file, so we need to make a copy
                if (scanningContext.FileStorageManager != null)
                {
                    string newPath = scanningContext.FileStorageManager.NextFilePath() +
                                     Path.GetExtension(serializedImage.FilePath);
                    File.Copy(serializedImage.FilePath, newPath);
                    storage = new ImageFileStorage(newPath);
                }
                else
                {
                    var data = File.ReadAllBytes(serializedImage.FilePath);
                    var typeHint = Path.GetExtension(serializedImage.FilePath).ToLowerInvariant();
                    storage = new ImageMemoryStorage(data, typeHint);
                }
            }
        }
        else
        {
            var data = serializedImage.FileContent.ToByteArray();
            storage = new ImageMemoryStorage(data, serializedImage.TypeHint);
        }

        var processedImage = scanningContext.CreateProcessedImage(
            storage,
            (BitDepth) serializedImage.Metadata.BitDepth,
            serializedImage.Metadata.Lossless,
            -1,
            serializedImage.Metadata.TransformListXml.FromXml<List<Transform>>());

        var thumbnail = serializedImage.Thumbnail.ToByteArray();
        if (thumbnail.Length > 0)
        {
            processedImage = processedImage.WithPostProcessingData(new PostProcessingData
            {
                Thumbnail = scanningContext.ImageContext.Load(new MemoryStream(thumbnail)),
                ThumbnailTransformState = processedImage.TransformState
            }, true);
        }
        return processedImage;
    }
}