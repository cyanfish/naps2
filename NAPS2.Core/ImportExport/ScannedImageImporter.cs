using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport
{
    public class ScannedImageImporter : IScannedImageImporter
    {
        private readonly IScannedImageImporter pdfImporter;
        private readonly IScannedImageImporter imageImporter;

        public ScannedImageImporter(IPdfImporter pdfImporter, IImageImporter imageImporter)
        {
            this.pdfImporter = pdfImporter;
            this.imageImporter = imageImporter;
        }

        public IEnumerable<ScannedImage> Import(string filePath, ImportParams importParams, Func<int, int, bool> progressCallback)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            switch (Path.GetExtension(filePath).ToLowerInvariant())
            {
                case ".pdf":
                    return pdfImporter.Import(filePath, importParams, progressCallback);
                default:
                    return imageImporter.Import(filePath, importParams, progressCallback);
            }
        }
    }
}
