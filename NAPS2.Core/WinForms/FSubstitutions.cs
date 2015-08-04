using System;
using System.Collections.Generic;
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

            // We have to populate the sub button texts manually to avoid localization.
            // Annoying, but oh well.
            var subs = new[]
            {
                FileNameSubstitution.YEAR_4_DIGITS,
                FileNameSubstitution.YEAR_2_DIGITS,
                FileNameSubstitution.MONTH_2_DIGITS,
                FileNameSubstitution.DAY_2_DIGITS,
                FileNameSubstitution.HOUR_24_CLOCK,
                FileNameSubstitution.MINUTE_2_DIGITS,
                FileNameSubstitution.SECOND_2_DIGITS,
                FileNameSubstitution.NUMBER_4_DIGITS,
                FileNameSubstitution.NUMBER_3_DIGITS,
                FileNameSubstitution.NUMBER_2_DIGITS,
                FileNameSubstitution.NUMBER_1_DIGIT,
                string.Format("{0}-{1}-{2}", FileNameSubstitution.YEAR_4_DIGITS, FileNameSubstitution.MONTH_2_DIGITS, FileNameSubstitution.DAY_2_DIGITS),
                string.Format("{0}_{1}_{2}", FileNameSubstitution.HOUR_24_CLOCK, FileNameSubstitution.MINUTE_2_DIGITS, FileNameSubstitution.SECOND_2_DIGITS),
            };
            var buttons = gboxSubs.Controls.OfType<Button>().OrderBy(x => x.Top).ThenBy(x => x.Left).ToArray();
            for (int i = 0; i < Math.Min(subs.Length, buttons.Length); i++)
            {
                buttons[i].Text = subs[i];
            }
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
            lblPreview.Text = fileNameSubstitution.SubstituteFileName(txtFileName.Text, DateTime.Now, false);
        }
    }
}
