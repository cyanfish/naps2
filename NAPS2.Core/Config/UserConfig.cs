using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Batch;
using NAPS2.Scan.Images;

namespace NAPS2.Config
{
    public class UserConfig
    {
        public UserConfig()
        {
            FormStates = new List<FormState>();
            CustomPageSizePresets = new List<NamedPageSize>();
            ThumbnailSize = ThumbnailRenderer.DEFAULT_SIZE;
        }

        public const int CURRENT_VERSION = 2;

        public int Version { get; set; }

        public string Culture { get; set; }

        public List<FormState> FormStates { get; set; }

        public DateTime? LastUpdateCheckDate { get; set; }

        public bool EnableOcr { get; set; }

        public string OcrLanguageCode { get; set; }

        public string LastImageExt { get; set; }

        public PdfSettings PdfSettings { get; set; }

        public ImageSettings ImageSettings { get; set; }

        public EmailSettings EmailSettings { get; set; }

        public int ThumbnailSize { get; set; }

        public BatchSettings LastBatchSettings { get; set; }

        public DockStyle DesktopToolStripDock { get; set; }

        public KeyboardShortcuts KeyboardShortcuts { get; set; }

        public List<NamedPageSize> CustomPageSizePresets { get; set; }
    }
}
