using System.Collections.Immutable;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Oauth;
using NAPS2.ImportExport.Images;
using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Batch;

namespace NAPS2.Config;

public static class InternalDefaults
{
    public static CommonConfig GetCommonConfig() =>
        new CommonConfig
        {
            Version = CommonConfig.CURRENT_VERSION,
            Culture = "en",
            FormStates = ImmutableList<FormState>.Empty,
            BackgroundOperations = ImmutableHashSet<string>.Empty,
            CustomPageSizePresets = ImmutableList<NamedPageSize>.Empty,
            StartupMessageTitle = "",
            StartupMessageText = "",
            StartupMessageIcon = MessageBoxIcon.None,
            ScanChangesDefaultProfile = true,
            ScanButtonDefaultAction = ScanButtonDefaultAction.ScanWithDefaultProfile,
            SaveButtonDefaultAction = SaveButtonDefaultAction.SaveAll,
            HiddenButtons = ToolbarButtons.None,
            DisableAutoSave = false,
            LockSystemProfiles = false,
            LockUnspecifiedDevices = false,
            NoUserProfiles = false,
            AlwaysRememberDevice = false,
            NoUpdatePrompt = false,
            CheckForUpdates = false,
            HasCheckedForUpdates = false,
            LastUpdateCheckDate = null,
            HasBeenRun = false,
            FirstRunDate = null,
            HasBeenPromptedForDonation = false,
            LastDonatePromptDate = null,
            DeleteAfterSaving = false,
            DisableSaveNotifications = false,
            DisableExitConfirmation = false,
            SingleInstance = false,
            ComponentsPath = "",
            OcrTimeoutInSeconds = 10 * 60, // 10 minutes
            EnableOcr = false,
            OcrLanguageCode = "",
            LastOcrMultiLangCode = "",
            OcrMode = LocalizedOcrMode.Fast,
            OcrPreProcessing = false,
            OcrAfterScanning = true,
            LastImageExt = "",
            LastPdfOrImageExt = "",
            ThumbnailSize = ThumbnailSizes.DEFAULT_SIZE,
            DesktopToolStripDock = DockStyle.Top,
            EventLogging = EventType.None,
            ShowPageNumbers = false,
            SidebarVisible = true,
            PdfSettings = new PdfSettings
            {
                Metadata = new PdfMetadata
                {
                    Title = MiscResources.ScannedImage,
                    Subject = MiscResources.ScannedImage,
                    Author = MiscResources.NAPS2,
                    Creator = MiscResources.NAPS2,
                    Keywords = ""
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
                SkipSavePrompt = false,
                SinglePagePdfs = false
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
                AttachmentName = "Scan.pdf"
            },
            RememberEmailSettings = false,
            EmailSetup = new EmailSetup
            {
                SystemProviderName = "",
                ProviderType = EmailProviderType.System,
                SmtpUser = "",
                GmailUser = "",
                OutlookWebToken = new OauthToken(),
                SmtpHost = "",
                GmailToken = new OauthToken(),
                OutlookWebUser = "",
                SmtpFrom = "",
                SmtpPassword = "",
                SmtpPort = 0,
                SmtpTls = false
            },
            BatchSettings = new BatchSettings
            {
                OutputType = BatchOutputType.Load,
                SaveSeparator = SaveSeparator.FilePerPage,
                ScanType = BatchScanType.Single,
                SavePath = "",
                ScanIntervalSeconds = 0,
                ScanCount = 1,
                ProfileDisplayName = ""
            },
            KeyboardShortcuts = new KeyboardShortcuts
            {
                ScanDefault = "Mod+Enter",
                ScanProfile1 = "F2",
                ScanProfile2 = "F3",
                ScanProfile3 = "F4",
                ScanProfile4 = "F5",
                ScanProfile5 = "F6",
                ScanProfile6 = "F7",
                ScanProfile7 = "F8",
                ScanProfile8 = "F9",
                ScanProfile9 = "F10",
                ScanProfile10 = "F11",
                ScanProfile11 = "F12",
                ScanProfile12 = "",
                NewProfile = "Mod+N",
                BatchScan = "Mod+B",
                Profiles = "Mod+L",
                ScannerSharing = "Mod+G",
                Ocr = "Mod+Alt+O",
                Import = "Mod+O",
                SavePDF = "",
                SavePDFAll = "Mod+S",
                SavePDFSelected = "Mod+Shift+S",
                PDFSettings = "Mod+Alt+P",
                SaveImages = "",
                SaveImagesAll = "Mod+I",
                SaveImagesSelected = "Mod+Shift+I",
                ImageSettings = "Mod+Alt+I",
                EmailPDF = "",
                EmailPDFAll = "Mod+E",
                EmailPDFSelected = "Mod+Shift+E",
                EmailSettings = "Mod+Alt+E",
                Print = "Mod+P",
                ImageView = "",
                ImageBlackWhite = "",
                ImageBrightness = "",
                ImageContrast = "",
                ImageCrop = "",
                ImageHue = "",
                ImageReset = "",
                ImageSaturation = "",
                ImageSharpen = "",
                RotateLeft = "Mod+Shift+Left",
                RotateRight = "Mod+Shift+Right",
                RotateFlip = "Mod+Shift+Down",
                RotateCustom = "",
                MoveUp = "Mod+Up",
                MoveDown = "Mod+Down",
                ReorderInterleave = "",
                ReorderDeinterleave = "",
                ReorderAltInterleave = "",
                ReorderAltDeinterleave = "",
                ReorderReverseAll = "",
                ReorderReverseSelected = "",
                Delete = "",
                Clear = "Mod+Shift+Del",
                About = "F1",
                ZoomIn = "Mod+Oemplus",
                ZoomOut = "Mod+OemMinus"
            },
            DefaultProfileSettings = new ScanProfile { Version = ScanProfile.CURRENT_VERSION }
        };
}