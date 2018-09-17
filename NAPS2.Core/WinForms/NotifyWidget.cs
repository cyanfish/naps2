using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class NotifyWidget : NotifyWidgetBase
    {
        private readonly string linkTarget;
        private readonly string folderTarget;

        public NotifyWidget(string title, string linkLabel, string linkTarget, string folderTarget)
        {
            this.linkTarget = linkTarget;
            this.folderTarget = folderTarget;
            InitializeComponent();

            lblTitle.Text = title;
            linkLabel1.Text = linkLabel;

            if (lblTitle.Width > Width - 35)
            {
                Width = lblTitle.Width + 35;
            }
            if (lblTitle.Height > Height - 35)
            {
                Height = lblTitle.Height + 35;
            }

            if (folderTarget == null)
            {
                contextMenuStrip1.Enabled = false;
            }
        }

        private void hideTimer_Tick(object sender, EventArgs e)
        {
            DoHideNotify();
        }

        protected void DoHideNotify()
        {
            InvokeHideNotify();
            hideTimer.Stop();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DoHideNotify();
        }

        private void NotifyWidget_MouseEnter(object sender, EventArgs e)
        {
            hideTimer.Stop();
        }

        private void NotifyWidget_MouseLeave(object sender, EventArgs e)
        {
            hideTimer.Start();
        }

        public override void ShowNotify()
        {
            hideTimer.Start();
        }

        protected virtual void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(linkLabel1, linkLabel1.Location);
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = linkTarget,
                    Verb = "open"
                });
            }
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
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
