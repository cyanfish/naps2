using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FRecover : FormBase
    {
        public FRecover()
        {
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = btnRecover;
            CancelButton = btnCancel;
        }

        public void SetData(int imageCount, DateTime scannedDateTime)
        {
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
