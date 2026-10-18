using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

internal class SettingsForm : EtoDialogBase
{
    private readonly GeneralSettingsPanel _panel;
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };

    public SettingsForm(Naps2Config config, DesktopFormProvider desktopFormProvider, IIconProvider iconProvider)
        : base(config)
    {
        Title = UiStrings.SettingsFormTitle;
        IconName = "cog_small";

        _panel = new GeneralSettingsPanel(this, desktopFormProvider)
        {
            KeyboardShortcutsCommand = new ActionCommand(() => FormFactory.Create<KeyboardShortcutsForm>().ShowModal())
            {
                Text = UiStrings.KeyboardShortcuts,
                Image = iconProvider.GetIcon("keyboard_small")
            }
        };
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