using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public class KeyboardShortcutsForm : EtoDialogBase
{
    private readonly KeyboardShortcutsPanel _panel;
    private readonly Button _restoreDefaults = C.Button(UiStrings.RestoreDefaults);

    public KeyboardShortcutsForm(Naps2Config config, KeyboardShortcutManager ksm,
        DesktopFormProvider desktopFormProvider) : base(config)
    {
        _panel = new KeyboardShortcutsPanel(this, ksm, desktopFormProvider);
        _restoreDefaults.Click += (_, _) => _panel.RestoreDefaults();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _panel.OnShown();
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.KeyboardShortcutsFormTitle;
        IconName = "keyboard_small";

        LayoutController.Content = L.Column(
            _panel.BuildLayout().Scale(),
            L.Row(
                _restoreDefaults.MinWidth(140),
                C.Filler(),
                L.OkCancel(C.OkButton(this, _panel.Save), C.CancelButton(this))
            )
        );
    }
}