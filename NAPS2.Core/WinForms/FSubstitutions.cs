using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAPS2.ImportExport;

namespace NAPS2.WinForms
{
    public partial class FSubstitutions : FormBase
    {
        private readonly FileNameSubstitution fileNameSubstitution;

        public FSubstitutions(FileNameSubstitution fileNameSubstitution)
        {
            this.fileNameSubstitution = fileNameSubstitution;
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }

        public string FileName { get; set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            txtFileName.Text = FileName;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            FileName = txtFileName.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void subButton_Click(object sender, EventArgs e)
        {
            var sub = ((Button)sender).Text;
            var cursorPos = txtFileName.SelectionStart + txtFileName.SelectionLength;
            txtFileName.Text = txtFileName.Text.Insert(cursorPos, sub);
            txtFileName.Select(cursorPos + sub.Length, 0);
            txtFileName.Focus();
        }

        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            lblPreview.Text = fileNameSubstitution.SubstituteFileName(txtFileName.Text, false);
        }
    }
}
