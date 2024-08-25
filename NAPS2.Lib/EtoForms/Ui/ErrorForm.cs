using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public class ErrorForm : EtoDialogBase
{
    private readonly ImageView _image = new();
    private readonly Label _message = new();
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
                _message.DynamicWrap(350).NaturalWidth(350).AlignCenter().Scale()
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

    public string Details
    {
        get => _details.Text;
        set => _details.Text = value;
    }
}