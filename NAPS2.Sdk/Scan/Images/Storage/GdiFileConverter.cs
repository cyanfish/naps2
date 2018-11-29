using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public class GdiFileConverter
    {
        [StorageConverter]
        public FileStorage ConvertToFile(GdiStorage input, StorageConvertParams convertParams)
        {
            if (convertParams.Temporary)
            {
                var path = Path.Combine(Paths.Temp, Path.GetRandomFileName());
                input.Bitmap.Save(path);
                return new FileStorage(path);
            }
            else
            {
                // TODO: Save smallest
                string ext = convertParams.Lossless ? ".png" : ".jpg";
                var path = FileStorageManager.Default.NextFilePath() + ext;
                input.Bitmap.Save(path);
                return new FileStorage(path);
            }
        }

        [StorageConverter]
        public GdiStorage ConvertToGdi(FileStorage input, StorageConvertParams convertParams) => new GdiStorage(new Bitmap(input.FullPath));

        [StorageConverter]
        public MemoryStreamStorage ConvertToMemoryStream(GdiStorage input, StorageConvertParams convertParams)
        {
            var stream = new MemoryStream();
            // TODO: Better format choice?
            var format = convertParams.Lossless ? ImageFormat.Png : ImageFormat.Jpeg;
            input.Bitmap.Save(stream, format);
            return new MemoryStreamStorage(stream);
        }
    }
}
