using NAPS2.Scan.Internal.Sane.Native;

namespace NAPS2.Scan.Internal.Sane;

internal class SaneOptionController
{
    private readonly SaneDevice _device;
    private Dictionary<string, SaneOption> _options = null!;

    public SaneOptionController(SaneDevice device)
    {
        _device = device;
        LoadOptions();
    }

    private void LoadOptions()
    {
        _options = _device.GetOptions()
            .Where(x => x.Name != null && x.Type != SaneValueType.Group)
            .ToDictionary(x => x.Name!);
    }

    public bool TrySet(string name, double value)
    {
        Console.WriteLine($"Maybe setting {name}");
        if (!_options.ContainsKey(name))
            return false;
        var opt = _options[name];
        if (!opt.IsActive || !opt.IsSettable || opt.Type is not (SaneValueType.Int or SaneValueType.Fixed))
            return false;
        try
        {
            Console.WriteLine($"Setting {name} to {value}");
            _device.SetOption(_options[name], value, out var info);
            if (info.HasFlag(SaneOptionSetInfo.ReloadOptions))
            {
                LoadOptions();
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.ErrorException($"Error writing SANE option {name}", ex);
            return false;
        }
    }

    public bool TrySet(string name, IEnumerable<string> valueSet)
    {
        Console.WriteLine($"Maybe setting {name}");
        if (!_options.ContainsKey(name))
            return false;
        var opt = _options[name];
        if (!opt.IsActive || !opt.IsSettable || opt.Type != SaneValueType.String)
            return false;
        try
        {
            var valueHashSet = valueSet.ToHashSet();
            foreach (var value in opt.StringList!)
            {
                if (valueHashSet.Contains(value))
                {
                    Console.WriteLine($"Setting {name} to {value}");
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
            Log.ErrorException($"Error writing SANE option {name}", ex);
        }
        return false;
    }
}