using System;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FRecover : FormBase
    {
        public FRecover()
        {
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = BtnRecover;
            CancelButton = BtnCancel;
        }

        public void SetData(int imageCount, DateTime scannedDateTime)
        {
            lblPrompt.Text = string.Format(lblPrompt.Text, imageCount, scannedDateTime.ToShortDateString(), scannedDateTime.ToShortTimeString());
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        private void BtnRecover_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}