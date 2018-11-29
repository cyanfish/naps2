using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public class GdiFileConverter :
        IStorageConverter<GdiStorage, FileStorage>,
        IStorageConverter<FileStorage, GdiStorage>
    {
        private readonly FileStorageManager fileStorageManager;

        public GdiFileConverter(FileStorageManager fileStorageManager)
        {
            this.fileStorageManager = fileStorageManager;
        }

        public FileStorage Convert(GdiStorage input, StorageConvertParams convertParams)
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
                var path = fileStorageManager.NextFilePath() + ext;
                input.Bitmap.Save(path);
                return new FileStorage(path);
            }
        }

        public GdiStorage Convert(FileStorage input, StorageConvertParams convertParams) => new GdiStorage(new Bitmap(input.FullPath));
    }
}
