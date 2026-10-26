using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.ImportExport;

namespace NAPS2.EtoForms.Ui;

public class PlaceholdersForm : EtoDialogBase
{
    private static readonly (string val, string text)[] PlaceholderButtons = {
            (NameManager.YEAR_4_DIGITS, UiStrings.Year4Digit),
            (NameManager.YEAR_2_DIGITS, UiStrings.Year2Digit),
            (NameManager.MONTH_2_DIGITS, UiStrings.Month2Digit),
            (NameManager.DAY_2_DIGITS, UiStrings.Day2Digit),
            (NameManager.HOUR_24_CLOCK, UiStrings.Hour2Digit),
            (NameManager.MINUTE_2_DIGITS, UiStrings.Minute2Digit),
            (NameManager.SECOND_2_DIGITS, UiStrings.Second2Digit),
            (NameManager.NUMBER_4_DIGITS, UiStrings.AutoIncrementing4Digit),
            (NameManager.NUMBER_3_DIGITS, UiStrings.AutoIncrementing3Digit),
            (NameManager.NUMBER_2_DIGITS, UiStrings.AutoIncrementing2Digit),
            (NameManager.NUMBER_1_DIGIT, UiStrings.AutoIncrementing1Digit)
        };

    private readonly NameManager _nameManager;

    private readonly TextBox _fileName = new();
    private readonly Label _preview = new() { Text = " " };

    public PlaceholdersForm(Naps2Config config, NameManager nameManager) : base(config)
    {
        _nameManager = nameManager;
        // TODO: Ellipsis aren't working, presumably because Eto uses custom label rendering on WinForms
        _fileName.TextChanged += FileName_TextChanged;
    }

    protected override void BuildLayout()
    {
        _fileName.Text = FileName;

        Title = UiStrings.PlaceholdersFormTitle;

        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;

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
                        C.Button(NameManager.FULL_DATE, () => Add(NameManager.FULL_DATE)).MinWidth(150),
                        C.Button(NameManager.FULL_TIME, () => Add(NameManager.FULL_TIME)).MinWidth(150)
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
        _preview.Text = _nameManager.Get(_fileName.Text, new NameContext { DateTime = DateTime.Now, IsFilePath = false });
    }

}