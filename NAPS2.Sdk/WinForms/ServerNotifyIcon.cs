using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public class ServerNotifyIcon
    {
        public ServerNotifyIcon(int port, Action close)
        {
            NotifyIcon = new NotifyIcon
            {
                Icon = Icons.scanner_icon,
                BalloonTipIcon = ToolTipIcon.Info,
                BalloonTipText = string.Format(MiscResources.ListeningOnPort, port),
                BalloonTipTitle = MiscResources.ServerStarted,
                Text = MiscResources.Naps2Server,
                ContextMenuStrip = new ContextMenuStrip
                {
                    Items =
                    {
                        new ToolStripLabel(string.Format(MiscResources.ListeningOnPort, port)) { Enabled = false },
                        new ToolStripMenuItem(MiscResources.Close, null, (sender, args) => close())
                    }
                }
            };
        }

        public NotifyIcon NotifyIcon { get; }

        public void Show()
        {
            NotifyIcon.Visible = true;
            NotifyIcon.ShowBalloonTip(5000);
        }

        public void Hide()
        {
            NotifyIcon.Visible = false;
        }
    }
}
