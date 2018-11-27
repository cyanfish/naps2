using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public class GdiFileConverter : IStorageConverter<GdiStorage, FileStorage>, IStorageConverter<FileStorage, GdiStorage>
    {
        private readonly FileStorageManager fileStorageManager;

        public GdiFileConverter(FileStorageManager fileStorageManager)
        {
            this.fileStorageManager = fileStorageManager;
        }

        public FileStorage Convert(GdiStorage input, StorageConvertParams convertParams)
        {
            // TODO: Save smallest
            string ext = convertParams.HighQuality ? ".png" : ".jpg";
            var path = fileStorageManager.NextFilePath() + ext;
            input.Bitmap.Save(path);
            return new FileStorage(fileStorageManager, path);
        }

        public GdiStorage Convert(FileStorage input, StorageConvertParams convertParams) => new GdiStorage(new Bitmap(input.FullPath));
    }
}
