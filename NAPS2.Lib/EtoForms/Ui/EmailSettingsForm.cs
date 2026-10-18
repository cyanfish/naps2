using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

internal class EmailSettingsForm : EtoDialogBase
{
    private readonly EmailSettingsPanel _panel;
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };

    public EmailSettingsForm(Naps2Config config) :
        base(config)
    {
        Title = UiStrings.EmailSettingsFormTitle;
        IconName = "email_small";

        _panel = new EmailSettingsPanel(this);
        _restoreDefaults.Click += (_, _) => _panel.RestoreDefaults();
    }

    protected override void BuildLayout()
    {
        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            _panel.BuildLayout().Scale(),
            L.Row(
                _restoreDefaults.MinWidth(140),
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, _panel.Save),
                    C.CancelButton(this))
            )
        );
    }
}