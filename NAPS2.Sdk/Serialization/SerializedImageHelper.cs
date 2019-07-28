using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Scan;

namespace NAPS2.Serialization
{
    public static class SerializedImageHelper
    {
        public static SerializedImage Serialize(ImageContext imageContext, ScannedImage image, SerializeOptions options) =>
            Serialize(imageContext, image, image.Metadata, options);

        public static SerializedImage Serialize(ImageContext imageContext, ScannedImage.Snapshot snapshot, SerializeOptions options) =>
            Serialize(imageContext, snapshot.Source, snapshot.Metadata, options);

        private static SerializedImage Serialize(ImageContext imageContext, ScannedImage image, IImageMetadata metadata, SerializeOptions options)
        {
            MemoryStream thumbStream = null;
            var thumb = image.GetThumbnail();
            if (thumb != null && options.IncludeThumbnail)
            {
                thumbStream = imageContext.Convert<MemoryStreamStorage>(thumb, new StorageConvertParams { Lossless = true }).Stream;
            }

            var fileStorage = image.BackingStorage as FileStorage;
            if (fileStorage == null && options.RequireFileStorage)
            {
                throw new InvalidOperationException("FileStorage is required for serialization.");
            }

            MemoryStream imageStream = null;
            if (fileStorage == null)
            {
                imageStream = imageContext.Convert<MemoryStreamStorage>(image.BackingStorage, new StorageConvertParams()).Stream;
            }

            var result = new SerializedImage
            {
                TransferOwnership = options.TransferOwnership,
                Metadata = new SerializedImageMetadata
                {
                    TransformListXml = metadata.TransformList.ToXml(),
                    BitDepth = (SerializedImageMetadata.Types.BitDepth) metadata.BitDepth,
                    Lossless = metadata.Lossless
                },
                Thumbnail = thumbStream != null ? ByteString.FromStream(thumbStream) : ByteString.Empty,
                RenderedFilePath = options.RenderedFilePath ?? ""
            };
            if (fileStorage != null)
            {
                result.FilePath = fileStorage.FullPath;
            }
            else
            {
                result.FileContent = ByteString.FromStream(imageStream);
            }
            return result;
        }

        public static ScannedImage Deserialize(ImageContext imageContext, SerializedImage serializedImage, DeserializeOptions options)
        {
            IStorage storage;
            if (!string.IsNullOrEmpty(serializedImage.FilePath))
            {
                if (serializedImage.TransferOwnership)
                {
                    storage = new FileStorage(serializedImage.FilePath);
                }
                else if (options.ShareFileStorage)
                {
                    storage = new FileStorage(serializedImage.FilePath, true);
                }
                else
                {
                    // Not transfering or sharing the file, so we need to make a copy
                    string newPath = imageContext.FileStorageManager.NextFilePath();
                    File.Copy(serializedImage.FilePath, newPath);
                    storage = new FileStorage(newPath);
                }
            }
            else
            {
                var memoryStream = new MemoryStream(serializedImage.FileContent.ToByteArray());
                storage = new MemoryStreamStorage(memoryStream);
            }

            var backingStorage = imageContext.ConvertToBacking(storage, new StorageConvertParams
            {
                Lossless = serializedImage.Metadata.Lossless,
                BitDepth = (BitDepth) serializedImage.Metadata.BitDepth
            });
            var metadata = imageContext.ImageMetadataFactory.CreateMetadata(backingStorage);
            metadata.TransformList = serializedImage.Metadata.TransformListXml.FromXml<List<Transform>>();
            metadata.BitDepth = (BitDepth) serializedImage.Metadata.BitDepth;
            metadata.Lossless = serializedImage.Metadata.Lossless;
            metadata.Commit();

            var scannedImage = imageContext.CreateScannedImage(backingStorage, metadata, new StorageConvertParams());
            var thumbnail = serializedImage.Thumbnail.ToByteArray();
            if (thumbnail.Length > 0)
            {
                var thumbnailStorage = new MemoryStreamStorage(new MemoryStream(thumbnail));
                scannedImage.SetThumbnail(imageContext.ConvertToImage(thumbnailStorage, new StorageConvertParams()));
            }
            return scannedImage;
        }

        public class SerializeOptions
        {
            public bool TransferOwnership { get; set; }

            public bool IncludeThumbnail { get; set; }

            public bool RequireFileStorage { get; set; }

            public string RenderedFilePath { get; set; }
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
}
