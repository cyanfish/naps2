using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAPS2.Scan.Images;
using Ninject;

namespace NAPS2.ImportExport
{
    public class ScannedImageImporter : IScannedImageImporter
    {
        private readonly IScannedImageImporter pdfImporter;
        private readonly IScannedImageImporter imageImporter;

        public ScannedImageImporter([Named("pdf")] IScannedImageImporter pdfImporter, [Named("image")] IScannedImageImporter imageImporter)
        {
            this.pdfImporter = pdfImporter;
            this.imageImporter = imageImporter;
        }

        public IEnumerable<IScannedImage> Import(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
            switch (Path.GetExtension(filePath).ToLowerInvariant())
            {
                case ".pdf":
                    return pdfImporter.Import(filePath);
                default:
                    return imageImporter.Import(filePath);
            }
        }
    }
}
