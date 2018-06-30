using System;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FUpdate : FormBase
    {
        public FUpdate()
        {
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = BtnInstall;
            CancelButton = BtnCancel;
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void BtnDownload_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}