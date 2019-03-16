using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

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
        public GdiImage ConvertToGdi(FileStorage input, StorageConvertParams convertParams) => new GdiImage(new Bitmap(input.FullPath));

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
