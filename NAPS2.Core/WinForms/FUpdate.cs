using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FUpdate : FormBase
    {
        public FUpdate()
        {
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = btnInstall;
            CancelButton = btnCancel;
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
