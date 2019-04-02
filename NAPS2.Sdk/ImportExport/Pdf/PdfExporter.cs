using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Config.Experimental;
using NAPS2.Ocr;
using NAPS2.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport.Pdf
{
    public abstract class PdfExporter
    {
        private static PdfExporter _default = new PdfSharpExporter();

        public static PdfExporter Default
        {
            get
            {
                TestingContext.NoStaticDefaults();
                return _default;
            }
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        public abstract Task<bool> Export(string path, ICollection<ScannedImage.Snapshot> snapshots, ConfigProvider<PdfSettings> settings,
            OcrContext ocrContext, ProgressHandler progressCallback, CancellationToken cancelToken);
    }
}
