using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public class ErrorForm : EtoDialogBase
{
    private readonly ImageView _image = new();
    private readonly Label _message = new();
    private LinkButton? _link;
    private readonly TextArea _details = new() { ReadOnly = true };
    private readonly LayoutVisibility _detailsVisibility = new(false);

    public ErrorForm(Naps2Config config, IIconProvider iconProvider)
        : base(config)
    {
        _image.Image = iconProvider.GetIcon("exclamation");
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.ErrorFormTitle;

        FormStateController.RestoreFormState = false;
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            L.Row(
                _image.AlignCenter().Padding(right: 5),
                L.Column(
                    _message.DynamicWrap(350).NaturalWidth(350),
                    _link ?? C.None()
                ).AlignCenter().Scale()
            ),
            L.Row(
                C.Link(UiStrings.TechnicalDetails, ToggleDetails).AlignCenter(),
                C.Filler(),
                C.OkButton(this)
            ),
            _details.NaturalHeight(120).Visible(_detailsVisibility).Scale()
        );
    }

    private void ToggleDetails()
    {
        FormStateController.FixedHeightLayout = _detailsVisibility.IsVisible;
        _detailsVisibility.IsVisible = !_detailsVisibility.IsVisible;
    }

    public string ErrorMessage
    {
        get => _message.Text;
        set => _message.Text = value;
    }

    public string? Link
    {
        get => _link?.Text;
        set => _link = value == null ? null : C.UrlLink(value);
    }

    public string Details
    {
        get => _details.Text;
        set => _details.Text = value;
    }
}