using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.Util
{
    public class KeyboardShortcutManager
    {
        private readonly KeysConverter keysConverter = new KeysConverter();
        private readonly Dictionary<Keys, Action> dict = new Dictionary<Keys, Action>();

        public Keys Parse(string value)
        {
            return (Keys)(keysConverter.ConvertFrom(value) ?? Keys.None);
        }

        public bool Assign(string value, Action action)
        {
            var keys = Parse(value);
            if (keys != Keys.None)
            {
                dict[keys] = action;
                return true;
            }
            return false;
        }

        public bool Assign(string value, ToolStripMenuItem item, Action action)
        {
            var keys = Parse(value);
            if (keys != Keys.None)
            {
                try
                {
                    item.ShortcutKeys = keys;
                }
                catch (Exception)
                {
                    dict[keys] = action;
                }
                return true;
            }
            return false;
        }

        public bool Assign(string value, ToolStripButton item)
        {
            if (Assign(value, item.PerformClick))
            {
                item.AutoToolTip = true;
                item.ToolTipText = value;
                return true;
            }
            return false;
        }

        public bool Assign(string value, ToolStripMenuItem item)
        {
            return Assign(value, item, item.PerformClick);
        }

        public bool Assign(string value, ToolStripSplitButton item)
        {
            if (Assign(value, item.PerformClick))
            {
                item.AutoToolTip = true;
                item.ToolTipText = value;
                return true;
            }
            return false;
        }

        public bool Assign(string value, Button item)
        {
            return Assign(value, item.PerformClick);
        }

        public void Perform(Keys keyData)
        {
            if (dict.ContainsKey(keyData))
            {
                dict[keyData]();
            }
        }
    }
}
