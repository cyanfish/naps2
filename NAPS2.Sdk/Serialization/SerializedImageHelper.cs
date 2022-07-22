using Google.Protobuf;
using NAPS2.Scan;

namespace NAPS2.Serialization;

// TODO: Add tests for this class. Focus on use case tests (i.e. serialize + deserialize) rather than a bunch of tests to verify the generated proto. 
public static class SerializedImageHelper
{
    public static SerializedImage Serialize(ProcessedImage image, SerializeOptions options)
    {
        if (options.RequireFileStorage && options.RequireMemoryStorage)
        {
            throw new ArgumentException();
        }
        if (options.RequireFileStorage && image.Storage is not ImageFileStorage)
        {
            throw new InvalidOperationException("FileStorage is required for serialization.");
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
                if (options.RequireMemoryStorage)
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
        return result;
    }

    public static ProcessedImage Deserialize(ScanningContext scanningContext, SerializedImage serializedImage,
        DeserializeOptions options)
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
                // TODO: Handle no file storage
                string newPath = scanningContext.FileStorageManager.NextFilePath();
                File.Copy(serializedImage.FilePath, newPath);
                storage = new ImageFileStorage(newPath);
            }
        }
        else
        {
            var stream = new MemoryStream(serializedImage.FileContent.ToByteArray());
            storage = new ImageMemoryStorage(stream, serializedImage.TypeHint);
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

    public class SerializeOptions
    {
        public bool TransferOwnership { get; set; }

        public bool IncludeThumbnail { get; set; }

        public bool RequireFileStorage { get; set; }

        public bool RequireMemoryStorage { get; set; }

        public string? RenderedFilePath { get; set; }
    }

    public class DeserializeOptions
    {
        /// <summary>
        /// If true, the Deserialize caller guarantees that the file storage will not be used for longer than the duration of the RPC call.
        /// In this way, files can be safely reused even if ownership isn't transferred to the callee.
        /// This should not be true outside of an RPC context.
        /// </summary>
        public bool ShareFileStorage { get; set; }
    }
}