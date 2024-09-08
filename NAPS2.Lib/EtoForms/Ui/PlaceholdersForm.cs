using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.ImportExport;

namespace NAPS2.EtoForms.Ui;

public class PlaceholdersForm : EtoDialogBase
{
    private static readonly (string val, string text)[] PlaceholderButtons = {
            (Placeholders.YEAR_4_DIGITS, UiStrings.Year4Digit),
            (Placeholders.YEAR_2_DIGITS, UiStrings.Year2Digit),
            (Placeholders.MONTH_2_DIGITS, UiStrings.Month2Digit),
            (Placeholders.DAY_2_DIGITS, UiStrings.Day2Digit),
            (Placeholders.HOUR_24_CLOCK, UiStrings.Hour2Digit),
            (Placeholders.MINUTE_2_DIGITS, UiStrings.Minute2Digit),
            (Placeholders.SECOND_2_DIGITS, UiStrings.Second2Digit),
            (Placeholders.NUMBER_4_DIGITS, UiStrings.AutoIncrementing4Digit),
            (Placeholders.NUMBER_3_DIGITS, UiStrings.AutoIncrementing3Digit),
            (Placeholders.NUMBER_2_DIGITS, UiStrings.AutoIncrementing2Digit),
            (Placeholders.NUMBER_1_DIGIT, UiStrings.AutoIncrementing1Digit)
        };

    private readonly TextBox _fileName = new();
    private readonly Label _preview = new() { Text = " " };

    public PlaceholdersForm(Naps2Config config) : base(config)
    {
        // TODO: Ellipsis aren't working, presumably because Eto uses custom label rendering on WinForms
        _fileName.TextChanged += FileName_TextChanged;
    }

    protected override void BuildLayout()
    {
        _fileName.Text = FileName;

        Title = UiStrings.PlaceholdersFormTitle;

        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;
        base.BuildLayout();

        LayoutController.Content = L.Column(
            C.Label(UiStrings.FileNameLabel),
            _fileName,
            C.Label(UiStrings.PreviewLabel),
            _preview.Ellipsize(),
            L.Row(
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Save),
                    C.CancelButton(this))
            ),
            L.GroupBox(
                UiStrings.Placeholders,
                L.Column(
                    PlaceholderButtons.Select(x => L.Row(
                        C.Button(x.val, () => Add(x.val)),
                        C.Label(x.text)
                    )).Expand(),
                    L.Row(
                        C.Button(Placeholders.FULL_DATE, () => Add(Placeholders.FULL_DATE)).MinWidth(150),
                        C.Button(Placeholders.FULL_TIME, () => Add(Placeholders.FULL_TIME)).MinWidth(150)
                    )
                )
            )
        );
    }

    public string? FileName { get; set; }

    public bool Updated { get; private set; }

    private void Save()
    {
        FileName = _fileName.Text;
        Updated = true;
    }

    private void Add(string placeholderValue)
    {
        var cursorPos = _fileName.Selection.End + 1;
        _fileName.Text = _fileName.Text.Insert(cursorPos, placeholderValue);
        int newPos = cursorPos + placeholderValue.Length;
        _fileName.Selection = new Range<int>(newPos, newPos - 1);
        _fileName.Focus();
    }

    private void FileName_TextChanged(object? sender, EventArgs e)
    {
        _preview.Text = Placeholders.All.Substitute(_fileName.Text, false);
    }

}