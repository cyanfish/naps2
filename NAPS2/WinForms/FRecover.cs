using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FRecover : Form
    {
        public FRecover(int imageCount, DateTime scannedDateTime)
        {
            InitializeComponent();
            AcceptButton = btnRecover;
            CancelButton = btnCancel;
            lblPrompt.Text = string.Format(lblPrompt.Text, imageCount, scannedDateTime.ToShortDateString(), scannedDateTime.ToShortTimeString());
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        private void btnRecover_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
