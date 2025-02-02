using System.Text;
using Eto.Forms;

namespace NAPS2.EtoForms;

/// <summary>
/// A helper class to assign keyboard shortcuts to commands.
/// </summary>
public class KeyboardShortcutManager
{
    private readonly Dictionary<Keys, Action> _dict = new();
    private readonly Dictionary<Keys, Command> _commandDict = new();

    private readonly Dictionary<string, Keys> _parseMap = new()
    {
        { "mod", Application.Instance.CommonModifier },
        { "cmd", Keys.Application },
        { "ctrl", Keys.Control },
        { "del", Keys.Delete },
        { "ins", Keys.Insert },
        { "break", Keys.Pause },
        { "oemplus", Keys.Equal },
        { "oemminus", Keys.Minus },
        { "esc", Keys.Escape },
        { "\\", Keys.Backslash },
        { ",", Keys.Comma },
        { "=", Keys.Equal },
        { "`", Keys.Grave },
        { "-", Keys.Minus },
        { ".", Keys.Period },
        { "\"", Keys.Quote },
        { ";", Keys.Semicolon },
        { "/", Keys.Slash },
        { "[", Keys.LeftBracket },
        { "]", Keys.RightBracket },
        { "0", Keys.D0 },
        { "1", Keys.D1 },
        { "2", Keys.D2 },
        { "3", Keys.D3 },
        { "4", Keys.D4 },
        { "5", Keys.D5 },
        { "6", Keys.D6 },
        { "7", Keys.D7 },
        { "8", Keys.D8 },
        { "9", Keys.D9 }
    };

    private readonly Dictionary<Keys, string> _stringifyMap = new()
    {
        { Keys.Escape, "Esc" },
        { Keys.Backslash, "\\" },
        { Keys.Comma, "," },
        { Keys.Equal, "=" },
        { Keys.Grave, "`" },
        { Keys.Minus, "-" },
        { Keys.Period, "." },
        { Keys.Quote, "\"" },
        { Keys.Semicolon, ";" },
        { Keys.Slash, "/" },
        { Keys.LeftBracket, "[" },
        { Keys.RightBracket, "]" },
        { Keys.D0, "0" },
        { Keys.D1, "1" },
        { Keys.D2, "2" },
        { Keys.D3, "3" },
        { Keys.D4, "4" },
        { Keys.D5, "5" },
        { Keys.D6, "6" },
        { Keys.D7, "7" },
        { Keys.D8, "8" },
        { Keys.D9, "9" }
    };

    public void Clear()
    {
        _dict.Clear();
        _commandDict.Clear();
    }

    public Keys Parse(string? value)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var keys = Keys.None;
                foreach (var part in value!.Split('+').Select(x => x.Trim().ToLowerInvariant()))
                {
                    if (_parseMap.ContainsKey(part))
                    {
                        keys |= _parseMap[part];
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

    public string? Stringify(Keys keys)
    {
        if (keys == Keys.None)
        {
            return null;
        }
        var sb = new StringBuilder();
        if (keys.HasFlag(Keys.Application))
        {
            sb.Append(EtoPlatform.Current.IsMac ? "Cmd + " : "Win + ");
        }
        if (keys.HasFlag(Keys.Control))
        {
            sb.Append("Ctrl + ");
        }
        if (keys.HasFlag(Keys.Shift))
        {
            sb.Append("Shift + ");
        }
        if (keys.HasFlag(Keys.Alt))
        {
            sb.Append("Alt + ");
        }
        var keyWithoutMods = keys & ~Keys.ModifierMask;
        sb.Append(_stringifyMap.Get(keyWithoutMods) ?? keyWithoutMods.ToString());
        return sb.ToString();
    }

    public bool Assign(string? value, Action action)
    {
        var keys = Parse(value);
        if (keys != Keys.None)
        {
            _dict[keys] = action;
            if (_commandDict.ContainsKey(keys))
            {
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