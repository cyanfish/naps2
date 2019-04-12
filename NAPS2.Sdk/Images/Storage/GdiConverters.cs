using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NAPS2.ImportExport.Pdf;

namespace NAPS2.Images.Storage
{
    public class GdiConverters
    {
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
                var path = FileStorageManager.Current.NextFilePath() + ext;
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
                var renderer = new GhostscriptPdfRenderer(null);
                return new GdiImage(renderer.Render(input.FullPath).Single());
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
