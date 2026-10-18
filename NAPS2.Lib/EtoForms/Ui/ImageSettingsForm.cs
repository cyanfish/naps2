using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public class ImageSettingsForm : EtoDialogBase
{
    private readonly ImageSettingsPanel _panel;
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };

    public ImageSettingsForm(Naps2Config config, DialogHelper dialogHelper) : base(config)
    {
        Title = UiStrings.ImageSettingsFormTitle;
        IconName = "picture_small";

        _panel = new ImageSettingsPanel(this, dialogHelper);
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