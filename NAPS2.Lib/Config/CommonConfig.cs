using System.Collections.Immutable;
using NAPS2.Config.Model;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.Pdf;
using NAPS2.Ocr;
using NAPS2.Scan;
using NAPS2.Scan.Batch;

namespace NAPS2.Config;

[Config]
public class CommonConfig
{
    public const int CURRENT_VERSION = 3;

    [Common]
    public int? Version { get; set; } = CURRENT_VERSION;

    [Common]
    public string? Culture { get; set; }

    [User]
    public ImmutableList<FormState> FormStates { get; set; } = ImmutableList<FormState>.Empty;

    [User]
    public ImmutableHashSet<string> BackgroundOperations { get; set; } = ImmutableHashSet<string>.Empty;

    [Common]
    public ImmutableList<NamedPageSize> CustomPageSizePresets { get; set; } = ImmutableList<NamedPageSize>.Empty;

    [App]
    public string? StartupMessageTitle { get; set; }

    [App]
    public string? StartupMessageText { get; set; }

    [App]
    public MessageBoxIcon StartupMessageIcon { get; set; }

    [Common]
    public SaveButtonDefaultAction SaveButtonDefaultAction { get; set; }

    [Common]
    public ToolbarButtons HiddenButtons { get; set; }

    [App]
    public bool DisableAutoSave { get; set; }

    [App]
    public bool LockSystemProfiles { get; set; }

    [App]
    public bool LockUnspecifiedDevices { get; set; }

    [App]
    public bool NoUserProfiles { get; set; }

    [Common]
    public bool AlwaysRememberDevice { get; set; }

    [Common]
    public bool NoUpdatePrompt { get; set; }

    [Common]
    public bool CheckForUpdates { get; set; }

    [User]
    public bool HasCheckedForUpdates { get; set; }

    [User]
    public DateTime? LastUpdateCheckDate { get; set; }

    [User]
    public bool HasBeenRun { get; set; }

    [User]
    public DateTime? FirstRunDate { get; set; }

    [User]
    public bool HasBeenPromptedForDonation { get; set; }

    [User]
    public DateTime? LastDonatePromptDate { get; set; }

    [Common]
    public bool DeleteAfterSaving { get; set; }

    [Common]
    public bool DisableSaveNotifications { get; set; }

    [Common]
    public bool DisableExitConfirmation { get; set; }

    [Common]
    public bool SingleInstance { get; set; }

    [App]
    public string? ComponentsPath { get; set; }

    [Common]
    public double OcrTimeoutInSeconds { get; set; }

    [Common]
    public bool EnableOcr { get; set; }

    [Common]
    public string? OcrLanguageCode { get; set; }

    [Common]
    public LocalizedOcrMode OcrMode { get; set; }

    [Common]
    public bool OcrAfterScanning { get; set; }

    [User]
    public string? LastImageExt { get; set; }

    [User]
    public string? LastPdfOrImageExt { get; set; }

    [Common]
    public int ThumbnailSize { get; set; }

    [Common]
    public DockStyle DesktopToolStripDock { get; set; }

    [App]
    public EventType EventLogging { get; set; }

    [Config]
    [Common]
    public PdfSettings PdfSettings { get; set; } = new();

    [User]
    public bool RememberPdfSettings { get; set; }

    [Config]
    [Common]
    public ImageSettings ImageSettings { get; set; } = new();

    [User]
    public bool RememberImageSettings { get; set; }

    [Config]
    [Common]
    public EmailSettings EmailSettings { get; set; } = new();

    [User]
    public bool RememberEmailSettings { get; set; }

    [Config]
    [Common]
    public EmailSetup EmailSetup { get; set; } = new();

    [Config]
    [Common]
    public BatchSettings BatchSettings { get; set; } = new();

    [Config]
    [Common]
    public KeyboardShortcuts KeyboardShortcuts { get; set; } = new();

    [Common]
    public ScanProfile? DefaultProfileSettings { get; set; }

    [Common]
    public bool EnableDebugLogging { get; set; }
    
    [User]
    public bool EnableThumbnailText { get; set; }

}