using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Scan;
using NAPS2.Scan.Batch;

namespace NAPS2.Config.Experimental
{
    public class CommonConfig
    {
        public const int CURRENT_VERSION = 3;

        public static CommonConfig Create() => new CommonConfig { Version = CURRENT_VERSION };

        [Common]
        public int Version { get; set; }

        [Common]
        public string Culture { get; set; }

        [User]
        public List<FormState> FormStates { get; set; }

        [User]
        public HashSet<string> BackgroundOperations { get; set; }

        [Common]
        public List<NamedPageSize> CustomPageSizePresets { get; set; }

        [User]
        public List<ScanProxyConfig> SavedProxies { get; set; }

        [App]
        public string StartupMessageTitle { get; set; }

        [App]
        public string StartupMessageText { get; set; }

        [App]
        public MessageBoxIcon? StartupMessageIcon { get; set; }

        [Common]
        public SaveButtonDefaultAction? SaveButtonDefaultAction { get; set; }

        [Common]
        public ToolbarButtons? HiddenButtons { get; set; }

        [App]
        public bool? DisableAutoSave { get; set; }

        [App]
        public bool? LockSystemProfiles { get; set; }

        [App]
        public bool? LockUnspecifiedDevices { get; set; }

        [App]
        public bool? NoUserProfiles { get; set; }

        [Common]
        public bool? AlwaysRememberDevice { get; set; }

        [App]
        public bool? DisableGenericPdfImport { get; set; }

        [Common]
        public bool? NoUpdatePrompt { get; set; }

        [Common]
        public bool? CheckForUpdates { get; set; }

        [User]
        public bool? HasCheckedForUpdates { get; set; }

        [User]
        public DateTime? LastUpdateCheckDate { get; set; }

        [User]
        public bool? HasBeenRun { get; set; }

        [User]
        public DateTime? FirstRunDate { get; set; }

        [User]
        public bool? HasBeenPromptedForDonation { get; set; }

        [User]
        public DateTime? LastDonatePromptDate { get; set; }

        [Common]
        public bool? DeleteAfterSaving { get; set; }

        [Common]
        public bool? DisableSaveNotifications { get; set; }

        [Common]
        public bool? DisableExitConfirmation { get; set; }

        [Common]
        public bool? SingleInstance { get; set; }

        [App]
        public string ComponentsPath { get; set; }

        [Common]
        public double? OcrTimeoutInSeconds { get; set; }

        [Common]
        public bool? EnableOcr { get; set; }

        [Common]
        public string OcrLanguageCode { get; set; }

        [Common]
        public OcrMode? OcrMode { get; set; }

        [Common]
        public bool? OcrAfterScanning { get; set; }

        [User]
        public string LastImageExt { get; set; }

        [Common]
        public int? ThumbnailSize { get; set; }

        [Common]
        public DockStyle? DesktopToolStripDock { get; set; }

        [App]
        public EventType? EventLogging { get; set; }

        [Common]
        public PdfSettings PdfSettings { get; set; }

        [User]
        public bool? RememberPdfSettings { get; set; }

        [Common]
        public ImageSettings ImageSettings { get; set; }

        [User]
        public bool? RememberImageSettings { get; set; }

        [Common]
        public EmailSettings EmailSettings { get; set; }

        [User]
        public bool? RememberEmailSettings { get; set; }

        [Common]
        public EmailSetup EmailSetup { get; set; }

        [Common]
        public BatchSettings BatchSettings { get; set; }

        [Common]
        public KeyboardShortcuts KeyboardShortcuts { get; set; }

        [Common]
        public ScanProfile DefaultProfileSettings { get; set; }
    }
}
