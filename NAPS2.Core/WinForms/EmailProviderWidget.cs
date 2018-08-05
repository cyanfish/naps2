using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAPS2.ImportExport.Email;

namespace NAPS2.WinForms
{
    public partial class EmailProviderWidget : UserControl
    {
        public EmailProviderWidget()
        {
            InitializeComponent();
        }

        public IEmailProvider Provider { get; set; }

        public Image ProviderIcon
        {
            get => pboxIcon.Image;
            set => pboxIcon.Image = value;
        }

        public string ProviderName
        {
            get => lblName.Text;
            set => lblName.Text = value;
        }
    }
}
