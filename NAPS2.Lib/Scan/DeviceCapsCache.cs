using Eto.Drawing;
using NAPS2.Escl.Client;
using NAPS2.EtoForms;

namespace NAPS2.Scan;

public class DeviceCapsCache
{
    private const int ICON_SIZE = 48;

    private readonly Dictionary<DeviceKey, ScanCaps> _capsCache = new();
    private readonly Dictionary<string, Image> _iconCache = new();

    private readonly IScanPerformer _scanPerformer;
    private readonly ImageContext _imageContext;
    private readonly Naps2Config _config;

    public DeviceCapsCache(IScanPerformer scanPerformer, ImageContext imageContext, Naps2Config config)
    {
        _scanPerformer = scanPerformer;
        _imageContext = imageContext;
        _config = config;
    }

    public ScanCaps? GetCachedCaps(ScanProfile profile)
    {
        var key = GetDeviceKey(profile);
        return _capsCache.Get(key);
    }

    public async Task<ScanCaps?> QueryCaps(ScanProfile profile)
    {
        var key = GetDeviceKey(profile);
        if (!_capsCache.ContainsKey(key))
        {
            try
            {
                var caps = await _scanPerformer.GetCaps(profile);
                _capsCache[key] = caps;
            }
            catch (Exception ex)
            {
                Log.DebugException("Error getting device caps", ex);
                return null;
            }
        }
        return _capsCache[key];
    }

    public Image? GetCachedIcon(ScanDevice? device) => GetCachedIcon(device?.IconUri);

    public Image? GetCachedIcon(string? iconUri)
    {
        if (iconUri == null) return null;
        return _iconCache.Get(iconUri);
    }

    public async Task<Image?> LoadIcon(ScanDevice? device) => await LoadIcon(device?.IconUri);

    public async Task<Image?> LoadIcon(string? iconUri)
    {
        if (iconUri == null) return null;
        if (!_iconCache.ContainsKey(iconUri))
        {
            try
            {
                var icon = await DoLoadIcon(iconUri);
                _iconCache[iconUri] = icon;
            }
            catch (Exception ex)
            {
                Log.DebugException($"Error loading device icon from {iconUri}", ex);
                return null;
            }
        }
        return _iconCache[iconUri];
    }

    private async Task<Image> DoLoadIcon(string iconUri)
    {
        var client = EsclClient.GetHttpClient(_config.Get(c => c.EsclSecurityPolicy));
        var imageBytes = await client.GetByteArrayAsync(iconUri);
        using var image = _imageContext.Load(imageBytes);
        return image.PerformTransform(new ThumbnailTransform(ICON_SIZE)).ToEtoImage();
    }

    private DeviceKey GetDeviceKey(ScanProfile profile)
    {
        if (profile.DriverName == null || profile.Device == null) throw new ArgumentException();
        return new DeviceKey(profile.DriverName, profile.Device.ID, profile.WiaVersion, profile.TwainImpl);
    }

    private record DeviceKey(string DriverName, string DeviceId, WiaApiVersion WiaVersion, TwainImpl TwainImpl);
}