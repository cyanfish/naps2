using System.Collections.Immutable;
using NAPS2.Config.Model;
using NAPS2.Config.ObsoleteTypes;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.Config;

public class ProfileSerializer : VersionedSerializer<ConfigStorage<ImmutableList<ScanProfile>>>
{
    private readonly ConfigStorageSerializer<ImmutableList<ScanProfile>> _serializer = new();
    
    protected override void InternalSerialize(Stream stream, ConfigStorage<ImmutableList<ScanProfile>> obj)
    {
        _serializer.Serialize(stream, obj);
    }

    protected override ConfigStorage<ImmutableList<ScanProfile>> InternalDeserialize(Stream stream, XDocument doc)
    {
        return new ConfigStorage<ImmutableList<ScanProfile>>(DeserializeProfiles(stream));
    }

    private ImmutableList<ScanProfile> DeserializeProfiles(Stream stream)
    {
        try
        {
            return ReadProfiles(stream);
        }
        catch (InvalidOperationException)
        {
            // Continue, and try to read using the old serializer now
            stream.Seek(0, SeekOrigin.Begin);
        }

        try
        {
            return ReadOldProfiles(stream);
        }
        catch (InvalidOperationException)
        {
            // Continue, and try to read using the older serializer now
            stream.Seek(0, SeekOrigin.Begin);
        }

        return ReadVeryOldProfiles(stream);
    }

    private ImmutableList<ScanProfile> ReadProfiles(Stream configFileStream)
    {
        var serializer = new XmlSerializer<ImmutableList<ScanProfile>>();
        var settingsList = serializer.Deserialize(configFileStream) ?? throw new InvalidOperationException();
        // Upgrade from v1 to v2 if necessary
        foreach (var settings in settingsList)
        {
            if (settings.Version == 1)
            {
                if (settings.DriverName == DriverNames.TWAIN)
                {
                    settings.UseNativeUI = true;
                }
                settings.Version = ScanProfile.CURRENT_VERSION;
                settings.UpgradedFrom = 1;
            }
        }
        return settingsList;
    }

    private ImmutableList<ScanProfile> ReadOldProfiles(Stream configFileStream)
    {
        var serializer = new XmlSerializer<List<ExtendedScanSettingsV0>>();
        var profiles = serializer.Deserialize(configFileStream) ?? throw new InvalidOperationException();
        // Upgrade from v1 to v2 if necessary
        foreach (var settings in profiles)
        {
            if (settings.Version == 1)
            {
                if (settings.DriverName == DriverNames.TWAIN)
                {
                    settings.UseNativeUI = true;
                }
            }
        }
        return profiles.Select(profile => new ScanProfile
        {
            Version = ScanProfile.CURRENT_VERSION,
            UpgradedFrom = profile.Version,
            Device = profile.Device != null ? new ScanDevice(profile.Device.ID, profile.Device.Name) : null,
            DriverName = profile.DriverName,
            DisplayName = profile.DisplayName ?? "",
            MaxQuality = profile.MaxQuality,
            IsDefault = profile.IsDefault,
            IconID = profile.IconID,
            AfterScanScale = profile.AfterScanScale,
            BitDepth = profile.BitDepth,
            Brightness = profile.Brightness,
            Contrast = profile.Contrast,
            CustomPageSize = profile.CustomPageSize,
            PageAlign = profile.PageAlign,
            PageSize = profile.PageSize,
            PaperSource = profile.PaperSource,
            Resolution = profile.Resolution,
            UseNativeUI = profile.UseNativeUI
        }).ToImmutableList();
    }

    private ImmutableList<ScanProfile> ReadVeryOldProfiles(Stream configFileStream)
    {
        // For compatibility with profiles.xml from old versions, load ScanSettingsV0 instead of ScanProfile (which is used exclusively now)
        var serializer = new XmlSerializer<List<ScanSettingsV0>>();
        var profiles = serializer.Deserialize(configFileStream) ?? throw new InvalidOperationException();

        return profiles.Select(profile =>
        {
            if (profile.DriverName == null && profile.Device != null)
            {
                // Copy the DriverName to the new property
                profile.DriverName = profile.Device.DriverName;
                // This old property is unused, so remove its value
                profile.Device.DriverName = null;
            }
            // Everything should be ScanProfile now
            var result = new ScanProfile
            {
                Version = ScanProfile.CURRENT_VERSION,
                UpgradedFrom = 0,
                Device = profile.Device != null ? new ScanDevice(profile.Device.ID, profile.Device.Name) : null,
                DriverName = profile.DriverName,
                DisplayName = profile.DisplayName ?? "",
                MaxQuality = profile.MaxQuality,
                IsDefault = profile.IsDefault,
                IconID = profile.IconID,
                // If the driver is WIA and the profile type is not Extended, that meant the native UI was to be used
                UseNativeUI = profile.DriverName == DriverNames.WIA
            };
            if (profile is ExtendedScanSettingsV0 ext)
            {
                result.AfterScanScale = ext.AfterScanScale;
                result.BitDepth = ext.BitDepth;
                result.Brightness = ext.Brightness;
                result.Contrast = ext.Contrast;
                result.CustomPageSize = ext.CustomPageSize;
                result.PageAlign = ext.PageAlign;
                result.PageSize = ext.PageSize;
                result.PaperSource = ext.PaperSource;
                result.Resolution = ext.Resolution;
                result.UseNativeUI = ext.UseNativeUI;
            }
            return result;
        }).ToImmutableList();
    }
}