using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAPS2.ImportExport.Email;

namespace NAPS2.WinForms
{
    public partial class EmailProviderWidget : Button
    {
        public EmailProviderWidget()
        {
            InitializeComponent();
        }

        public EmailProviderType ProviderType { get; set; }

        public Image ProviderIcon
        {
            get => pboxIcon.Image;
            set => pboxIcon.Image = value;
        }

        public string ProviderName
        {
            get => Text;
            set => Text = value;
        }

        public Action ClickAction { get; set; }

        private void EmailProviderWidget_MouseEnter(object sender, EventArgs e)
        {
            BackColor = Color.FromArgb(229, 241, 251);
        }

        private void EmailProviderWidget_MouseLeave(object sender, EventArgs e)
        {
            BackColor = DefaultBackColor;
        }

        private void EmailProviderWidget_Click(object sender, EventArgs e)
        {
            ClickAction?.Invoke();
        }
    }
}
