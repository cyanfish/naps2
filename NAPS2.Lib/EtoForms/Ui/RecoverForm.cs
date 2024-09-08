using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.Recovery;

namespace NAPS2.EtoForms.Ui;

public class RecoverForm : EtoDialogBase
{
    private readonly Label _prompt = new();

    public RecoverForm(Naps2Config config) : base(config)
    {
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.RecoverFormTitle;

        FormStateController.SaveFormState = false;
        FormStateController.RestoreFormState = false;
        // FormStateController.Resizable = false;

        var recoverButton = C.DialogButton(this, UiStrings.Recover,
            beforeClose: () => SelectedAction = RecoverAction.Recover);
        var deleteButton = C.DialogButton(this, UiStrings.Delete,
            beforeClose: () => SelectedAction = RecoverAction.Delete);
        var notNowButton = C.CancelButton(this, UiStrings.NotNow);

        LayoutController.Content = L.Column(
            _prompt.DynamicWrap(400).MinWidth(300),
            C.Filler(),
            L.Row(
                recoverButton.Scale().Height(32),
                deleteButton.Scale().Height(32),
                notNowButton.Scale().Height(32)
            )
        );
    }

    public RecoverAction SelectedAction { get; private set; }

    public void SetData(int imageCount, DateTime scannedDateTime)
    {
        _prompt.Text = string.Format(UiStrings.RecoverPrompt, imageCount, scannedDateTime.ToShortDateString(),
            scannedDateTime.ToShortTimeString());
    }
}