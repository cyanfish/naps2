using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public class ErrorForm : EtoDialogBase
{
    private readonly ImageView _image = new();
    private readonly Label _message = new();
    private readonly TextArea _details = new() { ReadOnly = true };
    private readonly LayoutVisibility _detailsVisibility = new(false);
    private readonly TaskCompletionSource<bool> _retryTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public ErrorForm(Naps2Config config, IIconProvider iconProvider)
        : base(config)
    {
        EtoPlatform.Current.SetImageSize(_image, 32);
        EtoPlatform.Current.AttachDpiDependency(this,
            scale => _image.Image = iconProvider.GetIcon("exclamation", IconScenario.Inline32, scale));
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
                ShowRetry
                    ? L.OkCancel(
                        C.DialogButton(this, UiStrings.Retry, isDefault: true,
                            beforeClose: () => _retryTcs.TrySetResult(true)),
                        C.CancelButton(this))
                    : C.OkButton(this)
            ),
            _details.NaturalHeight(120).Visible(_detailsVisibility).Scale()
        );
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        // Any way of closing the form other than the Retry button means we don't retry
        _retryTcs.TrySetResult(false);
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

    /// <summary>
    /// Whether to show Retry/Cancel buttons instead of OK. Must be set before the form is shown.
    /// </summary>
    public bool ShowRetry { get; set; }

    /// <summary>
    /// Resolves to true if the user selected Retry, or false if the form was closed any other way.
    /// </summary>
    public Task<bool> RetryTask => _retryTcs.Task;
}