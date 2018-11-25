using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.ImportExport.Pdf;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Scan;

namespace NAPS2.Config
{
    public class AppConfig
    {
        public const int CURRENT_VERSION = 2;

        public int Version { get; set; }

        public string DefaultCulture { get; set; }

        public string StartupMessageTitle { get; set; }

        public string StartupMessageText { get; set; }

        public MessageBoxIcon StartupMessageIcon { get; set; }

        public ScanProfile DefaultProfileSettings { get; set; }

        public SaveButtonDefaultAction SaveButtonDefaultAction { get; set; }

        public bool HideOcrButton { get; set; }

        public bool HideImportButton { get; set; }

        public bool HideSavePdfButton { get; set; }

        public bool HideSaveImagesButton { get; set; }

        public bool HideEmailButton { get; set; }

        public bool HidePrintButton { get; set; }

        public bool HideDonateButton { get; set; }

        public bool DisableAutoSave { get; set; }

        public bool LockSystemProfiles { get; set; }

        public bool LockUnspecifiedDevices { get; set; }

        public bool NoUserProfiles { get; set; }

        public bool AlwaysRememberDevice { get; set; }

        public bool DisableGenericPdfImport { get; set; }

        public bool NoUpdatePrompt { get; set; }

        public bool DeleteAfterSaving { get; set; }

        public bool DisableSaveNotifications { get; set; }

        public bool SingleInstance { get; set; }

        public string ComponentsPath { get; set; }

        public double OcrTimeoutInSeconds { get; set; }

        public OcrState OcrState { get; set; }
        
        public string OcrDefaultLanguage { get; set; }

        public OcrMode OcrDefaultMode { get; set; }

        public bool OcrDefaultAfterScanning { get; set; }

        public PdfCompat ForcePdfCompat { get; set; } 

        public EventType EventLogging { get; set; }

        public KeyboardShortcuts KeyboardShortcuts { get; set; }
    }
}
