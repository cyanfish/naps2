using Eto.Drawing;
using NAPS2.Escl.Client;
using NAPS2.EtoForms;

namespace NAPS2.Scan;

public class DeviceCapsCache
{
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
        if (profile.DriverName == null || profile.Device == null) return null;
        var key = GetDeviceKey(profile);
        lock (_capsCache)
        {
            return _capsCache.Get(key);
        }
    }

    public async Task<ScanCaps?> QueryCaps(ScanProfile profile)
    {
        if (profile.DriverName == null || profile.Device == null) return null;
        var key = GetDeviceKey(profile);
        bool contains;
        lock (_capsCache)
        {
            contains = _capsCache.ContainsKey(key);
        }
        if (!contains)
        {
            try
            {
                var caps = await _scanPerformer.GetCaps(profile);
                lock (_capsCache)
                {
                    _capsCache[key] = caps;
                }
            }
            catch (Exception ex)
            {
                Log.DebugException("Error getting device caps", ex);
                return null;
            }
        }
        lock (_capsCache)
        {
            return _capsCache[key];
        }
    }

    public Image? GetCachedIcon(ScanDevice? device) => GetCachedIcon(device?.IconUri);

    public Image? GetCachedIcon(string? iconUri)
    {
        if (iconUri == null) return null;
        lock (_iconCache)
        {
            return _iconCache.Get(iconUri);
        }
    }

    public async Task<Image?> LoadIcon(ScanDevice? device) => await LoadIcon(device?.IconUri);

    public async Task<Image?> LoadIcon(string? iconUri)
    {
        if (iconUri == null) return null;
        bool contains;
        lock (_iconCache)
        {
            contains = _iconCache.ContainsKey(iconUri);
        }
        if (!contains)
        {
            try
            {
                var icon = await DoLoadIcon(iconUri);
                lock (_iconCache)
                {
                    _iconCache[iconUri] = icon;
                }
            }
            catch (Exception ex)
            {
                Log.DebugException($"Error loading device icon from {iconUri}", ex);
                return null;
            }
        }
        lock (_iconCache)
        {
            return _iconCache[iconUri];
        }
    }

    private async Task<Image> DoLoadIcon(string iconUri)
    {
        IMemoryImage? image;
        if (iconUri.StartsWith("file://"))
        {
            string path = new Uri(iconUri).LocalPath;
            image = _imageContext.Load(path);
        }
        else
        {
            var client = EsclClient.GetHttpClient(_config.Get(c => c.EsclSecurityPolicy));
            var imageBytes = await client.GetByteArrayAsync(iconUri);
            image = _imageContext.Load(imageBytes);
        }
        return image.ToEtoImage();
    }

    private DeviceKey GetDeviceKey(ScanProfile profile)
    {
        return new DeviceKey(profile.DriverName!, profile.Device!.ID, profile.WiaVersion, profile.TwainImpl);
    }

    private record DeviceKey(string DriverName, string DeviceId, WiaApiVersion WiaVersion, TwainImpl TwainImpl);
}