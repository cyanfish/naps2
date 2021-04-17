using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace NAPS2.Images.Storage
{
    public class GdiConverters
    {
        private readonly ImageContext _imageContext;

        public GdiConverters(ImageContext imageContext)
        {
            _imageContext = imageContext;
        }
        
        // TODO: I've gotten rid of most of the craziness. What remains is file/stream -> image and image -> file/stream.
        // TODO: So it probably makes more sense to get rid of the "converters" and use imagecontext/iimage methods instead.
        // TODO: That also might make lifetime easier to reason about.
        
        [StorageConverter]
        public FileStorage ConvertToFile(GdiImage input, StorageConvertParams convertParams)
        {
            if (convertParams.Temporary)
            {
                var path = Path.Combine(Paths.Temp, Path.GetRandomFileName());
                input.Bitmap.Save(path);
                return new FileStorage(path);
            }
            else
            {
                var tempPath = ScannedImageHelper.SaveSmallestBitmap(input.Bitmap, convertParams.BitDepth, convertParams.Lossless, convertParams.LossyQuality, out ImageFormat fileFormat);
                string ext = Equals(fileFormat, ImageFormat.Png) ? ".png" : ".jpg";
                var path = _imageContext.FileStorageManager.NextFilePath() + ext;
                File.Move(tempPath, path);
                return new FileStorage(path);
            }
        }

        [StorageConverter]
        public GdiImage ConvertToGdi(FileStorage input, StorageConvertParams convertParams)
        {
            // TODO: Allow multiple converters (with priority?) and fall back to the next if it returns null
            // Then we can have a PDF->Image converter that returns null if it's not a pdf file.
            if (IsPdfFile(input))
            {
                return (GdiImage)_imageContext.PdfRenderer.Render(input.FullPath, 300).Single();
            }
            else
            {
                return new GdiImage(new Bitmap(input.FullPath));
            }
        }

        private static bool IsPdfFile(FileStorage fileStorage) => Path.GetExtension(fileStorage.FullPath)?.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase) ?? false;

        [StorageConverter]
        public GdiImage ConvertToGdi(MemoryStreamStorage input, StorageConvertParams convertParams) => new GdiImage(new Bitmap(input.Stream));

        [StorageConverter]
        public MemoryStreamStorage ConvertToMemoryStream(GdiImage input, StorageConvertParams convertParams)
        {
            var stream = new MemoryStream();
            // TODO: Better format choice?
            var format = convertParams.Lossless ? ImageFormat.Png : ImageFormat.Jpeg;
            input.Bitmap.Save(stream, format);
            stream.Seek(0, SeekOrigin.Begin);
            return new MemoryStreamStorage(stream);
        }
    }
}
