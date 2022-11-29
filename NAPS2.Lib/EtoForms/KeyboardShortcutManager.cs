using Eto.Forms;

namespace NAPS2.EtoForms;

/// <summary>
/// A helper class to assign keyboard shortcuts to commands.
/// </summary>
public class KeyboardShortcutManager
{
    private readonly Dictionary<Keys, Action> _dict = new();
    private readonly Dictionary<Keys, Command> _commandDict = new();

    private readonly Dictionary<string, Keys> _customMap = new()
    {
        { "ctrl", Keys.Control },
        { "del", Keys.Delete },
        { "ins", Keys.Insert },
        { "break", Keys.Pause },
        { "oemplus", Keys.Equal },
        { "oemminus", Keys.Minus },
    }; 

    public Keys Parse(string? value)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var keys = Keys.None;
                foreach (var part in value!.Split('+').Select(x => x.Trim().ToLowerInvariant()))
                {
                    if (_customMap.ContainsKey(part))
                    {
                        keys |= _customMap[part];
                    }
                    else
                    {
                        keys |= (Keys) Enum.Parse(typeof (Keys), part, true);
                    }
                }
                return keys;
            }
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error parsing keyboard shortcut", ex);
        }
        return Keys.None;
    }

    public bool Assign(string? value, Action action)
    {
        var keys = Parse(value);
        if (keys != Keys.None)
        {
            _dict[keys] = action;
            if (_commandDict.ContainsKey(keys))
            {
                ActionCommand c;
                _commandDict[keys].Shortcut = Keys.None;
                _commandDict.Remove(keys);
            }
            return true;
        }
        return false;
    }

    public bool Assign(string? value, Command command, Action action)
    {
        var keys = Parse(value);
        if (keys != Keys.None)
        {
            try
            {
                command.Shortcut = keys;
                _commandDict[keys] = command;
                _dict[keys] = action;
            }
            catch (Exception)
            {
                _dict[keys] = action;
                if (_commandDict.ContainsKey(keys))
                {
                    _commandDict[keys].Shortcut = Keys.None;
                    _commandDict.Remove(keys);
                }
            }
            return true;
        }
        return false;
    }

    public bool Assign(string? value, Command command)
    {
        return Assign(value, command, command.Execute);
    }

    public bool Perform(Keys keyData)
    {
        if (_dict.ContainsKey(keyData))
        {
            _dict[keyData]();
            return true;
        }
        return false;
    }
}