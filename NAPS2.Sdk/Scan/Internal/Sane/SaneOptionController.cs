using Microsoft.Extensions.Logging;
using NAPS2.Scan.Internal.Sane.Native;

namespace NAPS2.Scan.Internal.Sane;

internal class SaneOptionController
{
    private readonly ISaneDevice _device;
    // TODO: Move exception handling + logging out if we split NAPS2.Sane off into a separate library
    private readonly ILogger _logger;
    private Dictionary<string, SaneOption> _options = null!;

    public SaneOptionController(ISaneDevice device, ILogger logger)
    {
        _device = device;
        _logger = logger;
        LoadOptions();
        foreach (var opt in _options.Values.OrderBy(x => x.Index))
        {
            _logger.LogDebug($"Option: {opt}");
        }
    }

    private void LoadOptions()
    {
        _options = _device.GetOptions()
            .Where(x => x.Name != null && x.Type != SaneValueType.Group)
            .GroupBy(x => x.Name!)
            .ToDictionary(x => x.Key, x => x.First());
    }

    public bool TrySet(string name, double value)
    {
        _logger.LogDebug($"Maybe setting {name}");
        if (!_options.ContainsKey(name))
            return false;
        var opt = _options[name];
        if (!opt.IsActive || !opt.IsSettable || opt.Type is not (SaneValueType.Int or SaneValueType.Fixed))
            return false;
        try
        {
            _logger.LogDebug($"Setting {name} to {value}");
            _device.SetOption(_options[name], value, out var info);
            if (info.HasFlag(SaneOptionSetInfo.ReloadOptions))
            {
                LoadOptions();
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing SANE option {OptionName}", name);
            return false;
        }
    }

    public bool TrySet(string name, SaneOptionMatcher matcher)
    {
        _logger.LogDebug($"Maybe setting {name}");
        if (!_options.ContainsKey(name))
            return false;
        var opt = _options[name];
        if (!opt.IsActive || !opt.IsSettable || opt.Type != SaneValueType.String)
            return false;
        try
        {
            foreach (var value in opt.StringList!)
            {
                if (matcher.Matches(value))
                {
                    _logger.LogDebug($"Setting {name} to {value}");
                    _device.SetOption(opt, value, out var info);
                    if (info.HasFlag(SaneOptionSetInfo.ReloadOptions))
                    {
                        LoadOptions();
                    }
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing SANE option {OptionName}", name);
        }
        return false;
    }

    public SaneOption? GetOption(string name)
    {
        return _options.ContainsKey(name) ? _options[name] : null;
    }

    public bool TryGet(string name, out double value)
    {
        value = 0;
        if (!_options.ContainsKey(name))
            return false;
        var opt = _options[name];
        if (!opt.IsActive || opt.Type is not (SaneValueType.Int or SaneValueType.Fixed))
            return false;
        try
        {
            _device.GetOption(_options[name], out value);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing SANE option {OptionName}", name);
            return false;
        }
    }
}