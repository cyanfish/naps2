using NAPS2.ImportExport;
using System;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FPlaceholders : FormBase
    {
        private readonly FileNamePlaceholders fileNamePlaceholders;

        public FPlaceholders(FileNamePlaceholders fileNamePlaceholders)
        {
            this.fileNamePlaceholders = fileNamePlaceholders;
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = BtnOK;
            CancelButton = BtnCancel;
        }

        public string FileName { get; set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            TxtFileName.Text = FileName;

            // We have to populate the placeholder button texts manually to avoid localization.
            // Annoying, but oh well.
            var placeholders = new[]
            {
                FileNamePlaceholders.YEAR_4_DIGITS,
                FileNamePlaceholders.YEAR_2_DIGITS,
                FileNamePlaceholders.MONTH_2_DIGITS,
                FileNamePlaceholders.DAY_2_DIGITS,
                FileNamePlaceholders.HOUR_24_CLOCK,
                FileNamePlaceholders.MINUTE_2_DIGITS,
                FileNamePlaceholders.SECOND_2_DIGITS,
                FileNamePlaceholders.NUMBER_4_DIGITS,
                FileNamePlaceholders.NUMBER_3_DIGITS,
                FileNamePlaceholders.NUMBER_2_DIGITS,
                FileNamePlaceholders.NUMBER_1_DIGIT,
                string.Format("{0}-{1}-{2}", FileNamePlaceholders.YEAR_4_DIGITS, FileNamePlaceholders.MONTH_2_DIGITS, FileNamePlaceholders.DAY_2_DIGITS),
                string.Format("{0}_{1}_{2}", FileNamePlaceholders.HOUR_24_CLOCK, FileNamePlaceholders.MINUTE_2_DIGITS, FileNamePlaceholders.SECOND_2_DIGITS),
            };
            var buttons = gboxPlaceholders.Controls.OfType<Button>().OrderBy(x => x.Top).ThenBy(x => x.Left).ToArray();
            for (int i = 0; i < Math.Min(placeholders.Length, buttons.Length); i++)
            {
                buttons[i].Text = placeholders[i];
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            FileName = TxtFileName.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void PhButton_Click(object sender, EventArgs e)
        {
            var ph = ((Button)sender).Text;
            var cursorPos = TxtFileName.SelectionStart + TxtFileName.SelectionLength;
            TxtFileName.Text = TxtFileName.Text.Insert(cursorPos, ph);
            TxtFileName.Select(cursorPos + ph.Length, 0);
            TxtFileName.Focus();
        }

        private void TxtFileName_TextChanged(object sender, EventArgs e)
        {
            lblPreview.Text = fileNamePlaceholders.SubstitutePlaceholders(TxtFileName.Text, DateTime.Now, false);
        }
    }
}