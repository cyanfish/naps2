using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FPdfPassword : FormBase
    {
        public FPdfPassword()
        {
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }

        public string FileName { get; set; }

        public string Password { get; private set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            lblPrompt.Text = string.Format(lblPrompt.Text, FileName);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Password = txtPassword.Text;
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
