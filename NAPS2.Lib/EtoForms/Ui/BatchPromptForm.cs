using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public class BatchPromptForm : EtoDialogBase
{
    private readonly Button _scanButton;

    public BatchPromptForm(Naps2Config config) : base(config)
    {
        var scanNextCommand = new ActionCommand(() =>
        {
            Result = true;
            Close();
        })
        {
            Text = UiStrings.Scan,
            IconName = "control_play_blue_small"
        };
        _scanButton = C.Button(scanNextCommand, ButtonImagePosition.Left);
        DefaultButton = _scanButton;
    }

    public int ScanNumber { get; set; }

    public bool Result { get; private set; }

    protected override void BuildLayout()
    {
        Title = UiStrings.BatchPromptFormTitle;

        FormStateController.SaveFormState = false;
        FormStateController.RestoreFormState = false;
        FormStateController.Resizable = false;
        base.BuildLayout();

        LayoutController.Content = L.Column(
            C.Label(string.Format(UiStrings.ReadyForScan, ScanNumber)).NaturalWidth(200),
            C.Filler(),
            L.Row(
                L.OkCancel(
                    _scanButton.Scale(),
                    C.CancelButton(this, UiStrings.Done).Scale())
            )
        );
    }
}