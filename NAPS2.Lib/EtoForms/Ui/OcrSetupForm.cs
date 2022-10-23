using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.Ocr;

namespace NAPS2.EtoForms.Ui;

public class OcrSetupForm : EtoDialogBase
{
    private readonly CheckBox _enableOcr = C.CheckBox(UiStrings.MakePdfsSearchable);
    private readonly DropDown _ocrLang = new();
    private readonly DropDown _ocrMode = C.EnumDropDown(LocalizedOcrMode.Fast, LocalizedOcrMode.Best);
    private readonly CheckBox _ocrAfterScanning = C.CheckBox(UiStrings.RunOcrAfterScanning);
    private readonly LinkButton _moreLanguages = C.Link(UiStrings.GetMoreLanguages);
    private readonly Button _ok = C.Button(UiStrings.OK);
    private readonly Button _cancel = C.Button(UiStrings.Cancel);

    public OcrSetupForm(Naps2Config config) : base(config)
    {
        Title = UiStrings.OcrSetupFormTitle;
        Icon = new Icon(1f, Icons.text_small.ToEtoImage());
        Resizable = false;

        LayoutController.Content = L.Column(
            _enableOcr,
            L.Row(
                C.Label(UiStrings.OcrLanguageLabel).AlignCenter().Padding(right: 40),
                _ocrLang.XScale()
            ).Aligned(),
            L.Row(
                C.Label(UiStrings.OcrModeLabel).AlignCenter().Padding(right: 40),
                _ocrMode.XScale()
            ).Aligned(),
            _ocrAfterScanning,
            C.Filler(),
            L.Row(
                _moreLanguages.AlignCenter().Padding(right: 30),
                C.Filler(),
                _ok,
                _cancel
            )
        );
    }
}