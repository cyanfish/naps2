using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class NotifyWidget : UserControl
    {
        private readonly string LinkTarget;
        private readonly string folderTarget;

        public NotifyWidget(string title, string LinkLabel, string LinkTarget, string folderTarget)
        {
            this.LinkTarget = LinkTarget;
            this.folderTarget = folderTarget;
            InitializeComponent();

            lblTitle.Text = title;
            LinkLabel1.Text = LinkLabel;

            if (lblTitle.Width > Width - 35)
            {
                Width = lblTitle.Width + 35;
            }
            if (lblTitle.Height > Height - 35)
            {
                Height = lblTitle.Height + 35;
            }

            contextMenuStrip1.Enabled &= folderTarget != null;
        }

        public event EventHandler HideNotify;

        private void HideTimer_Tick(object sender, EventArgs e)
        {
            DoHideNotify();
        }

        private void DoHideNotify()
        {
            HideNotify?.Invoke(this, EventArgs.Empty);
            HideTimer.Stop();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            DoHideNotify();
        }

        private void NotifyWidget_MouseEnter(object sender, EventArgs e)
        {
            HideTimer.Stop();
        }

        private void NotifyWidget_MouseLeave(object sender, EventArgs e)
        {
            HideTimer.Start();
        }

        public void ShowNotify()
        {
            HideTimer.Start();
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(LinkLabel1, LinkLabel1.Location);
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = LinkTarget,
                    Verb = "open"
                });
            }
        }

        private void OpenFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = folderTarget,
                Verb = "open"
            });
        }
    }
}