using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Scan;
using NAPS2.Scan.Batch;

namespace NAPS2.Config.Experimental
{
    public static class InternalDefaults
    {
        // TODO: Test that no properties are null
        public static CommonConfig GetCommonConfig() =>
            new CommonConfig
            {
                Culture = "en",
                FormStates = new List<FormState>(),
                BackgroundOperations = new HashSet<string>(),
                CustomPageSizePresets = new List<NamedPageSize>(),
                SavedProxies = new List<ScanProxyConfig>(),
                StartupMessageTitle = "",
                StartupMessageText = "",
                StartupMessageIcon = MessageBoxIcon.None,
                SaveButtonDefaultAction = SaveButtonDefaultAction.SaveAll,
                HiddenButtons = ToolbarButtons.None,
                DisableAutoSave = false,
                LockSystemProfiles = false,
                LockUnspecifiedDevices = false,
                NoUserProfiles = false,
                AlwaysRememberDevice = false,
                DisableGenericPdfImport = false,
                NoUpdatePrompt = false,
                CheckForUpdates = false,
                HasCheckedForUpdates = false,
                LastUpdateCheckDate = DateTime.MinValue,
                HasBeenRun = false,
                FirstRunDate = DateTime.MinValue,
                HasBeenPromptedForDonation = false,
                LastDonatePromptDate = DateTime.MinValue,
                DeleteAfterSaving = false,
                DisableSaveNotifications = false,
                DisableExitConfirmation = false,
                SingleInstance = false,
                ComponentsPath = "",
                OcrTimeoutInSeconds = 10 * 60, // 10 minutes
                EnableOcr = false,
                OcrLanguageCode = "",
                OcrMode = OcrMode.Default,
                OcrAfterScanning = true,
                LastImageExt = "",
                ThumbnailSize = 128,
                DesktopToolStripDock = DockStyle.Top,
                EventLogging = EventType.None,
                PdfSettings = new PdfSettings
                {
                    Metadata = new PdfMetadata
                    {
                        Title = MiscResources.ScannedImage,
                        Subject = MiscResources.ScannedImage,
                        Author = MiscResources.NAPS2,
                        Creator = MiscResources.NAPS2
                    },
                    Encryption = new PdfEncryption
                    {
                        AllowAnnotations = false,
                        AllowContentCopying = false,
                        AllowContentCopyingForAccessibility = false,
                        AllowDocumentAssembly = false,
                        AllowDocumentModification = false,
                        AllowFormFilling = false,
                        AllowFullQualityPrinting = false,
                        AllowPrinting = false,
                        EncryptPdf = false,
                        OwnerPassword = "",
                        UserPassword = ""
                    },
                    Compat = PdfCompat.Default,
                    DefaultFileName = "",
                    SkipSavePrompt = false
                },
                RememberPdfSettings = false,
                ImageSettings = new ImageSettings
                {
                    DefaultFileName = "",
                    SkipSavePrompt = false,
                    JpegQuality = 75,
                    SinglePageTiff = false,
                    TiffCompression = TiffCompression.Auto
                },
                RememberImageSettings = false,
                EmailSettings = new EmailSettings
                {
                    AttachmentName = ""
                },
                RememberEmailSettings = false,
                EmailSetup = new EmailSetup(),
                BatchSettings = new BatchSettings(),
                KeyboardShortcuts = new KeyboardShortcuts(),
                DefaultProfileSettings = new ScanProfile { Version = ScanProfile.CURRENT_VERSION }
            };
    }
}