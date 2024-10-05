using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;
using System.Xml;
using NAPS2.Config.Model;
using NAPS2.Config.ObsoleteTypes;
using NAPS2.Escl;
using NAPS2.Pdf;
using NAPS2.Serialization;

namespace NAPS2.Config;

public class ConfigSerializer : VersionedSerializer<ConfigStorage<CommonConfig>>
{
    private readonly ConfigStorageSerializer<CommonConfig> _storageSerializer = new();
    private readonly ConfigReadMode _mode;
    private readonly ConfigRootName _rootName;

    public ConfigSerializer(ConfigReadMode mode, ConfigRootName rootName)
    {
        _mode = mode;
        _rootName = rootName;
    }

    protected override void InternalSerialize(Stream stream, ConfigStorage<CommonConfig> obj)
    {
        if (_mode != ConfigReadMode.All)
        {
            throw new NotSupportedException();
        }
        obj.Set(c => c.Version, CommonConfig.CURRENT_VERSION);
        _storageSerializer.Serialize(stream, obj, _rootName.ToString());
    }

    protected override ConfigStorage<CommonConfig> InternalDeserialize(Stream stream, XDocument doc)
    {
        if (GetVersion(doc) < 3)
        {
            if (_mode == ConfigReadMode.DefaultOnly)
            {
                var oldAppConfig = new XmlSerializer<AppConfigV0>().Deserialize(stream);
                return AppConfigV0ToCommonConfigDefault(
                    oldAppConfig ?? throw new InvalidOperationException("Couldn't parse app config"));
            }
            if (_mode == ConfigReadMode.LockedOnly)
            {
                var oldAppConfig = new XmlSerializer<AppConfigV0>().Deserialize(stream);
                return AppConfigV0ToCommonConfigLocked(
                    oldAppConfig ?? throw new InvalidOperationException("Couldn't parse app config"), doc);
            }
            var oldUserConfig = new XmlSerializer<UserConfigV0>().Deserialize(stream);
            return UserConfigV0ToCommonConfig(oldUserConfig ??
                                              throw new InvalidOperationException("Couldn't parse user config"));
        }
        if (_mode == ConfigReadMode.DefaultOnly)
        {
            FilterProperties(doc.Root!, "default", "default");
            return DeserializeXDoc(doc);
        }
        if (_mode == ConfigReadMode.LockedOnly)
        {
            FilterProperties(doc.Root!, "override", "default");
            return DeserializeXDoc(doc);
        }
        return _storageSerializer.Deserialize(stream);
    }

    private ConfigStorage<CommonConfig> DeserializeXDoc(XDocument doc)
    {
        var filteredStream = new MemoryStream();
        var xmlWriter = new XmlTextWriter(filteredStream, Encoding.UTF8);
        doc.WriteTo(xmlWriter);
        xmlWriter.Flush();
        filteredStream.Seek(0, SeekOrigin.Begin);
        return _storageSerializer.Deserialize(filteredStream);
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
            else if (childMode != target)
            {
                child.Remove();
            }
        }
    }

    private ConfigStorage<CommonConfig> AppConfigV0ToCommonConfigDefault(AppConfigV0 c)
    {
        var storage = new ConfigStorage<CommonConfig>();
        storage.Set(x => x.Version, CommonConfig.CURRENT_VERSION);
        storage.Set(x => x.Culture, c.DefaultCulture);
        storage.Set(x => x.StartupMessageTitle, c.StartupMessageTitle);
        storage.Set(x => x.StartupMessageText, c.StartupMessageText);
        storage.Set(x => x.StartupMessageIcon, c.StartupMessageIcon);
        storage.Set(x => x.DefaultProfileSettings, c.DefaultProfileSettings);
        storage.Set(x => x.ShowPageNumbers, c.ShowPageNumbers);
        storage.Set(x => x.ShowProfilesToolbar, c.ShowProfilesToolbar);
        storage.Set(x => x.ScanChangesDefaultProfile, c.ScanChangesDefaultProfile);
        storage.Set(x => x.ScanButtonDefaultAction, c.ScanButtonDefaultAction);
        storage.Set(x => x.SaveButtonDefaultAction, c.SaveButtonDefaultAction);
        storage.Set(x => x.DeleteAfterSaving, c.DeleteAfterSaving);
        storage.Set(x => x.KeepSession, c.KeepSession);
        storage.Set(x => x.SingleInstance, c.SingleInstance);
        storage.Set(x => x.HiddenButtons, GetHiddenButtonFlags(c));
        storage.Set(x => x.DisableAutoSave, c.DisableAutoSave);
        storage.Set(x => x.LockSystemProfiles, c.LockSystemProfiles);
        storage.Set(x => x.LockUnspecifiedDevices, c.LockUnspecifiedDevices);
        storage.Set(x => x.NoUserProfiles, c.NoUserProfiles);
        storage.Set(x => x.AlwaysRememberDevice, c.AlwaysRememberDevice);
        storage.Set(x => x.NoUpdatePrompt, c.NoUpdatePrompt);
        storage.Set(x => x.DisableSaveNotifications, c.DisableSaveNotifications);
        storage.Set(x => x.ComponentsPath, c.ComponentsPath);
        storage.Set(x => x.OcrTimeoutInSeconds, c.OcrTimeoutInSeconds);
        storage.Set(x => x.OcrLanguageCode, c.OcrDefaultLanguage);
        storage.Set(x => x.OcrMode, c.OcrDefaultMode);
        storage.Set(x => x.OcrAfterScanning, c.OcrDefaultAfterScanning);
        storage.Set(x => x.EsclSecurityPolicy, c.EsclSecurityPolicy);
        storage.Set(x => x.EsclServerCertificatePath, c.EsclServerCertificatePath);
        storage.Set(x => x.EventLogging, c.EventLogging);
        storage.Set(x => x.KeyboardShortcuts, MapKeyboardShortcuts(c.KeyboardShortcuts ?? new KeyboardShortcuts()));
        return storage;
    }

    private ConfigStorage<CommonConfig> AppConfigV0ToCommonConfigLocked(AppConfigV0 c, XDocument doc)
    {
        var storage = new ConfigStorage<CommonConfig>();
        if (c.OcrState == OcrState.Enabled)
        {
            storage.Set(x => x.EnableOcr, true);
        }
        if (c.OcrState == OcrState.Disabled)
        {
            storage.Set(x => x.EnableOcr, false);
        }
        if (c.ForcePdfCompat != PdfCompat.Default)
        {
            storage.Set(x => x.PdfSettings.Compat, c.ForcePdfCompat);
        }
        if (c.NoDebugLogging)
        {
            storage.Set(x => x.EnableDebugLogging, false);
        }
        if (c.NoScannerSharing)
        {
            storage.Set(x => x.DisableScannerSharing, true);
        }
        if (c.EsclSecurityPolicy != EsclSecurityPolicy.None)
        {
            storage.Set(x => x.EsclSecurityPolicy, c.EsclSecurityPolicy);
        }

        void SetIfLocked<T>(Expression<Func<CommonConfig, T>> accessor, T value, string name)
        {
            var element = doc.Root!.Element(name);
            bool isLocked = element?.Attribute("mode")?.Value == "lock";
            if (isLocked)
            {
                storage.Set(accessor, value);
            }
        }
        SetIfLocked(x => x.ShowPageNumbers, c.ShowPageNumbers, nameof(c.ShowPageNumbers));
        SetIfLocked(x => x.ShowProfilesToolbar, c.ShowProfilesToolbar, nameof(c.ShowProfilesToolbar));
        SetIfLocked(x => x.ScanChangesDefaultProfile, c.ScanChangesDefaultProfile, nameof(c.ScanChangesDefaultProfile));
        SetIfLocked(x => x.ScanButtonDefaultAction, c.ScanButtonDefaultAction, nameof(c.ScanButtonDefaultAction));
        SetIfLocked(x => x.SaveButtonDefaultAction, c.SaveButtonDefaultAction, nameof(c.SaveButtonDefaultAction));
        SetIfLocked(x => x.DeleteAfterSaving, c.DeleteAfterSaving, nameof(c.DeleteAfterSaving));
        SetIfLocked(x => x.KeepSession, c.KeepSession, nameof(c.KeepSession));
        SetIfLocked(x => x.SingleInstance, c.SingleInstance, nameof(c.SingleInstance));

        return storage;
    }

    private ToolbarButtons GetHiddenButtonFlags(AppConfigV0 c)
    {
        var flags = ToolbarButtons.None;
        if (c.HideOcrButton) flags |= ToolbarButtons.Ocr;
        if (c.HideImportButton) flags |= ToolbarButtons.Import;
        if (c.HideSavePdfButton) flags |= ToolbarButtons.SavePdf;
        if (c.HideSaveImagesButton) flags |= ToolbarButtons.SaveImages;
        if (c.HideEmailButton) flags |= ToolbarButtons.EmailPdf;
        if (c.HidePrintButton) flags |= ToolbarButtons.Print;
        if (c.HideSettingsButton) flags |= ToolbarButtons.Settings;
        if (c.HideDonateButton) flags |= ToolbarButtons.Donate;
        if (c.HideSidebar) flags |= ToolbarButtons.Sidebar;
        return flags;
    }

    private KeyboardShortcuts MapKeyboardShortcuts(KeyboardShortcuts keyboardShortcuts)
    {
        // To be platform-independent, replace "Ctrl+" with "Mod+", which means "Ctrl" on Window/Linux and "Cmd" on Mac
        var serializer = new XmlSerializer<KeyboardShortcuts>();
        var doc = serializer.SerializeToXDocument(keyboardShortcuts);
        foreach (var node in doc.Root!.Elements())
        {
            node.Value = node.Value.Replace("Ctrl+", "Mod+");
        }
        return serializer.DeserializeFromXDocument(doc)!;
    }

    private static ConfigStorage<CommonConfig> UserConfigV0ToCommonConfig(UserConfigV0 c)
    {
        var storage = new ConfigStorage<CommonConfig>();
        storage.Set(x => x.Version, CommonConfig.CURRENT_VERSION);
        storage.Set(x => x.Culture, c.Culture);
        // The new form states have different values (client size instead of form size), so best to go with defaults
        storage.Set(x => x.FormStates, ImmutableList<FormState>.Empty);
        if (c.BackgroundOperations != null)
        {
            storage.Set(x => x.BackgroundOperations, ImmutableHashSet.CreateRange(c.BackgroundOperations));
        }
        storage.Set(x => x.CheckForUpdates, c.CheckForUpdates);
        storage.Set(x => x.LastUpdateCheckDate, c.LastUpdateCheckDate);
        storage.Set(x => x.FirstRunDate, c.FirstRunDate);
        storage.Set(x => x.LastDonatePromptDate, c.LastDonatePromptDate);
        storage.Set(x => x.EnableOcr, c.EnableOcr);
        storage.Set(x => x.OcrLanguageCode, c.OcrLanguageCode);
        storage.Set(x => x.OcrMode, c.OcrMode);
        storage.Set(x => x.OcrAfterScanning, c.OcrAfterScanning);
        storage.Set(x => x.LastImageExt, c.LastImageExt);
        storage.Set(x => x.PdfSettings, c.PdfSettings);
        storage.Set(x => x.ImageSettings, c.ImageSettings);
        storage.Set(x => x.EmailSettings, c.EmailSettings);
        storage.Set(x => x.EmailSetup, c.EmailSetup);
        storage.Set(x => x.ThumbnailSize, c.ThumbnailSize);
        if (c.LastBatchSettings != null)
        {
            storage.Set(x => x.BatchSettings, c.LastBatchSettings);
        }
        storage.Set(x => x.DesktopToolStripDock, c.DesktopToolStripDock);
        if (c.KeyboardShortcuts != null)
        {
            storage.Set(x => x.KeyboardShortcuts, c.KeyboardShortcuts);
        }
        if (c.CustomPageSizePresets != null)
        {
            storage.Set(x => x.CustomPageSizePresets, ImmutableList.CreateRange(c.CustomPageSizePresets));
        }
        return storage;
    }
}