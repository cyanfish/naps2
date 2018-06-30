using System;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FPdfPassword : FormBase
    {
        public FPdfPassword()
        {
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = BtnOK;
            CancelButton = BtnCancel;
        }

        public string FileName { get; set; }

        public string Password { get; private set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            lblPrompt.Text = string.Format(lblPrompt.Text, FileName);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            Password = txtPassword.Text;
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