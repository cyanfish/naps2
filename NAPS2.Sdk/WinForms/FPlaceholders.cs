using System;
using System.Linq;
using System.Windows.Forms;
using NAPS2.ImportExport;

namespace NAPS2.WinForms;

public partial class FPlaceholders : FormBase
{
    public FPlaceholders()
    {
        RestoreFormState = false;
        InitializeComponent();
        AcceptButton = btnOK;
        CancelButton = btnCancel;
    }

    public string FileName { get; set; }

    protected override void OnLoad(object sender, EventArgs eventArgs)
    {
        txtFileName.Text = FileName;

        // We have to populate the placeholder button texts manually to avoid localization.
        // Annoying, but oh well.
        var placeholders = new[]
        {
            Placeholders.YEAR_4_DIGITS,
            Placeholders.YEAR_2_DIGITS,
            Placeholders.MONTH_2_DIGITS,
            Placeholders.DAY_2_DIGITS,
            Placeholders.HOUR_24_CLOCK,
            Placeholders.MINUTE_2_DIGITS,
            Placeholders.SECOND_2_DIGITS,
            Placeholders.NUMBER_4_DIGITS,
            Placeholders.NUMBER_3_DIGITS,
            Placeholders.NUMBER_2_DIGITS,
            Placeholders.NUMBER_1_DIGIT,
            $"{Placeholders.YEAR_4_DIGITS}-{Placeholders.MONTH_2_DIGITS}-{Placeholders.DAY_2_DIGITS}",
            $"{Placeholders.HOUR_24_CLOCK}_{Placeholders.MINUTE_2_DIGITS}_{Placeholders.SECOND_2_DIGITS}",
        };
        var buttons = gboxPlaceholders.Controls.OfType<Button>().OrderBy(x => x.Top).ThenBy(x => x.Left).ToArray();
        for (int i = 0; i < Math.Min(placeholders.Length, buttons.Length); i++)
        {
            buttons[i].Text = placeholders[i];
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

    private void phButton_Click(object sender, EventArgs e)
    {
        var ph = ((Button)sender).Text;
        var cursorPos = txtFileName.SelectionStart + txtFileName.SelectionLength;
        txtFileName.Text = txtFileName.Text.Insert(cursorPos, ph);
        txtFileName.Select(cursorPos + ph.Length, 0);
        txtFileName.Focus();
    }

    private void txtFileName_TextChanged(object sender, EventArgs e)
    {
        lblPreview.Text = Placeholders.All.Substitute(txtFileName.Text, false);
    }
}