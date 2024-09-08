using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class PdfPasswordForm : EtoDialogBase
{
    private readonly PasswordBoxWithToggle _passwordBox = new();

    public PdfPasswordForm(Naps2Config config)
        : base(config)
    {
    }

    public string? FileName { get; set; }

    public string? Password { get; private set; }

    public bool Result { get; private set; }

    protected override void BuildLayout()
    {
        Title = UiStrings.PdfPasswordFormTitle;

        FormStateController.RestoreFormState = false;
        FormStateController.FixedHeightLayout = true;

        _passwordBox.Title = FileName!;
        _passwordBox.TitleWrapWidth = 300;
        LayoutController.Content = L.Column(
            L.Row(
                C.Label(UiStrings.EncryptedFilePrompt),
                C.Spacer()
            ).Aligned(),
            _passwordBox.AsInlineControl(),
            L.Row(
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Submit),
                    C.CancelButton(this))
            )
        );
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _passwordBox.Focus();
    }

    private void Submit()
    {
        Password = _passwordBox.Text ?? "";
        Result = true;
    }
}