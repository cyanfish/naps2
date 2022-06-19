using System.Collections.Immutable;
using System.Text;
using System.Xml;
using NAPS2.Config.ObsoleteTypes;
using NAPS2.ImportExport.Pdf;
using NAPS2.Serialization;

namespace NAPS2.Config;

public class ConfigSerializer : VersionedSerializer<CommonConfig>
{
    private readonly ConfigReadMode _mode;

    public ConfigSerializer(ConfigReadMode mode)
    {
        _mode = mode;
    }

    protected override void InternalSerialize(Stream stream, CommonConfig obj)
    {
        if (_mode != ConfigReadMode.All)
        {
            throw new NotSupportedException();
        }
        XmlSerialize(stream, obj);
    }

    protected override CommonConfig InternalDeserialize(Stream stream, XDocument doc)
    {
        if (GetVersion(doc) < 3)
        {
            if (_mode == ConfigReadMode.DefaultOnly)
            {
                var oldAppConfig = XmlDeserialize<AppConfigV0>(stream);
                return AppConfigV0ToCommonConfigDefault(oldAppConfig);
            }
            if (_mode == ConfigReadMode.LockedOnly)
            {
                var oldAppConfig = XmlDeserialize<AppConfigV0>(stream);
                return AppConfigV0ToCommonConfigLocked(oldAppConfig);
            }
            var oldUserConfig = XmlDeserialize<UserConfigV0>(stream);
            return UserConfigV0ToCommonConfig(oldUserConfig);
        }
        if (_mode == ConfigReadMode.DefaultOnly)
        {
            FilterProperties(doc.Root, "default", "default");
            return DeserializeXDoc(doc);
        }
        if (_mode == ConfigReadMode.LockedOnly)
        {
            FilterProperties(doc.Root, "override", "default");
            return DeserializeXDoc(doc);
        }
        return XmlDeserialize(stream);
    }

    private CommonConfig DeserializeXDoc(XDocument doc)
    {
        var filteredStream = new MemoryStream();
        doc.WriteTo(new XmlTextWriter(filteredStream, Encoding.UTF8));
        filteredStream.Seek(0, SeekOrigin.Begin);
        return XmlDeserialize(filteredStream);
    }

    private void FilterProperties(XElement node, string target, string current)
    {
        foreach (var child in node.Elements().ToList())
        {
            var childMode = child.Attribute("mode")?.Value ?? current;
            if (child.HasElements)
            {
                FilterProperties(child, target, childMode);
            }
            else if(childMode != target)
            {
                child.Remove();
            }
        }
    }

    private CommonConfig AppConfigV0ToCommonConfigDefault(AppConfigV0 c) =>
        new CommonConfig
        {
            // Could maybe move the app-only properties to Locked scope, but it really shouldn't matter
            Version = CommonConfig.CURRENT_VERSION,
            Culture = c.DefaultCulture,
            StartupMessageTitle = c.StartupMessageTitle,
            StartupMessageText = c.StartupMessageText,
            StartupMessageIcon = c.StartupMessageIcon,
            DefaultProfileSettings = c.DefaultProfileSettings,
            SaveButtonDefaultAction = c.SaveButtonDefaultAction,
            HiddenButtons = GetHiddenButtonFlags(c),
            DisableAutoSave = c.DisableAutoSave,
            LockSystemProfiles = c.LockSystemProfiles,
            LockUnspecifiedDevices = c.LockUnspecifiedDevices,
            NoUserProfiles = c.NoUserProfiles,
            AlwaysRememberDevice = c.AlwaysRememberDevice,
            DisableGenericPdfImport = c.DisableGenericPdfImport,
            NoUpdatePrompt = c.NoUpdatePrompt,
            DeleteAfterSaving = c.DeleteAfterSaving,
            DisableSaveNotifications = c.DisableSaveNotifications,
            SingleInstance = c.SingleInstance,
            ComponentsPath = c.ComponentsPath,
            OcrTimeoutInSeconds = c.OcrTimeoutInSeconds,
            OcrLanguageCode = c.OcrDefaultLanguage,
            OcrMode = c.OcrDefaultMode,
            OcrAfterScanning = c.OcrDefaultAfterScanning,
            EventLogging = c.EventLogging,
            KeyboardShortcuts = c.KeyboardShortcuts ?? new KeyboardShortcuts()
        };

    private CommonConfig AppConfigV0ToCommonConfigLocked(AppConfigV0 c) =>
        new CommonConfig
        {
            EnableOcr = c.OcrState == OcrState.Enabled ? true
                : c.OcrState == OcrState.Disabled ? false
                : (bool?)null,
            PdfSettings =
            {
                Compat = c.ForcePdfCompat != PdfCompat.Default ? c.ForcePdfCompat : (PdfCompat?)null
            }
        };

    private ToolbarButtons? GetHiddenButtonFlags(AppConfigV0 c)
    {
        var flags = ToolbarButtons.None;
        if (c.HideOcrButton) flags |= ToolbarButtons.Ocr;
        if (c.HideImportButton) flags |= ToolbarButtons.Import;
        if (c.HideSavePdfButton) flags |= ToolbarButtons.SavePdf;
        if (c.HideSaveImagesButton) flags |= ToolbarButtons.SaveImages;
        if (c.HideEmailButton) flags |= ToolbarButtons.EmailPdf;
        if (c.HidePrintButton) flags |= ToolbarButtons.Print;
        if (c.HideDonateButton) flags |= ToolbarButtons.Donate;
        return flags;
    }

    private static CommonConfig UserConfigV0ToCommonConfig(UserConfigV0 c) =>
        new CommonConfig
        {
            Version = CommonConfig.CURRENT_VERSION,
            Culture = c.Culture,
            FormStates = ImmutableList.CreateRange(c.FormStates),
            BackgroundOperations = ImmutableHashSet.CreateRange(c.BackgroundOperations),
            CheckForUpdates = c.CheckForUpdates,
            LastUpdateCheckDate = c.LastUpdateCheckDate,
            FirstRunDate = c.FirstRunDate,
            LastDonatePromptDate = c.LastDonatePromptDate,
            EnableOcr = c.EnableOcr,
            OcrLanguageCode = c.OcrLanguageCode,
            OcrMode = c.OcrMode,
            OcrAfterScanning = c.OcrAfterScanning,
            LastImageExt = c.LastImageExt,
            PdfSettings = c.PdfSettings,
            ImageSettings = c.ImageSettings,
            EmailSettings = c.EmailSettings,
            EmailSetup = c.EmailSetup,
            ThumbnailSize = c.ThumbnailSize,
            BatchSettings = c.LastBatchSettings,
            DesktopToolStripDock = c.DesktopToolStripDock,
            KeyboardShortcuts = c.KeyboardShortcuts,
            CustomPageSizePresets = ImmutableList.CreateRange(c.CustomPageSizePresets),
            SavedProxies = ImmutableList.CreateRange(c.SavedProxies)
        };
}