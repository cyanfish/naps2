using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public class ErrorForm : EtoDialogBase
{
    private readonly Label _message = new();
    private readonly TextArea _details = new() { ReadOnly = true };
    private readonly LayoutVisibility _detailsVisibility = new(false);

    public ErrorForm(Naps2Config config)
        : base(config)
    {
        Title = UiStrings.ErrorFormTitle;
        FormStateController.RestoreFormState = false;

        var image = new ImageView { Image = Icons.exclamation.ToEtoImage() };

        FormStateController.FixedHeightLayout = true;
        LayoutController.Content = L.Column(
            L.Row(
                image.AlignCenter().Padding(right: 5),
                _message.Wrap(350).NaturalWidth(350).AlignCenter().XScale()
            ),
            L.Row(
                C.Link(UiStrings.TechnicalDetails, ToggleDetails).AlignCenter(),
                C.Filler(),
                C.OkButton(this)
            ),
            _details.NaturalHeight(120).Visible(_detailsVisibility).YScale()
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