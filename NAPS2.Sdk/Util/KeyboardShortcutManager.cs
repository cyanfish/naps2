using System.ComponentModel;
using System.Windows.Forms;

namespace NAPS2.Util;

// TODO: Refactor to Eto and move to NAPS2.EtoForms
/// <summary>
/// A helper class to assign keyboard shortcuts to actions or WinForm buttons.
/// </summary>
public class KeyboardShortcutManager
{
    private readonly Dictionary<Keys, Action> _dict = new Dictionary<Keys, Action>();
    private readonly Dictionary<Keys, ToolStripMenuItem> _itemDict = new Dictionary<Keys, ToolStripMenuItem>();

    private readonly Dictionary<string, Keys> _customMap = new Dictionary<string, Keys>
    {
        { "ctrl", Keys.Control },
        { "del", Keys.Delete },
        { "ins", Keys.Insert },
        { "break", Keys.Pause },
    }; 

    public Keys Parse(string? value)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var keys = Keys.None;
                foreach (var part in value.Split('+').Select(x => x.Trim().ToLowerInvariant()))
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
            if (_itemDict.ContainsKey(keys))
            {
                _itemDict[keys].ShortcutKeys = Keys.None;
                _itemDict.Remove(keys);
            }
            return true;
        }
        return false;
    }

    public bool Assign(string? value, ToolStripMenuItem item, Action action)
    {
        var keys = Parse(value);
        if (keys != Keys.None)
        {
            try
            {
                item.ShortcutKeys = Keys.None;
                item.ShortcutKeyDisplayString = TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString(keys);
                _itemDict[keys] = item;
                _dict[keys] = action;
            }
            catch (Exception)
            {
                _dict[keys] = action;
                if (_itemDict.ContainsKey(keys))
                {
                    _itemDict[keys].ShortcutKeys = Keys.None;
                    _itemDict.Remove(keys);
                }
            }
            return true;
        }
        return false;
    }

    public bool Assign(string? value, ToolStripButton item)
    {
        if (Assign(value, item.PerformClick))
        {
            item.AutoToolTip = true;
            item.ToolTipText = value;
            return true;
        }
        return false;
    }

    public bool Assign(string? value, ToolStripMenuItem item)
    {
        return Assign(value, item, item.PerformClick);
    }

    public bool Assign(string? value, ToolStripSplitButton item)
    {
        if (Assign(value, item.PerformButtonClick))
        {
            item.AutoToolTip = true;
            item.ToolTipText = value;
            return true;
        }
        return false;
    }

    public bool Assign(string? value, Button item)
    {
        return Assign(value, item.PerformClick);
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