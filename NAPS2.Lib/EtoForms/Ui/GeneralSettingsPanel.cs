using System.Linq.Expressions;
using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

internal class GeneralSettingsPanel : SettingsPanel
{
    private readonly DesktopFormProvider _desktopFormProvider;
    private readonly EnumDropDownWidget<Theme> _theme = new(scale: false);
    private readonly CheckBox _scanChangesDefaultProfile = C.CheckBox(UiStrings.ScanChangesDefaultProfile);
    private readonly CheckBox _showProfilesToolbar = C.CheckBox(UiStrings.ShowProfilesToolbar);
    private readonly CheckBox _showPageNumbers = C.CheckBox(UiStrings.ShowPageNumbers);
    private readonly EnumDropDownWidget<ScanButtonDefaultAction> _scanButtonDefaultAction = new(scale: false);
    private readonly EnumDropDownWidget<SaveButtonDefaultAction> _saveButtonDefaultAction = new(scale: false);
    private readonly CheckBox _clearAfterSaving = C.CheckBox(UiStrings.ClearAfterSaving);
    private readonly CheckBox _keepSession = C.CheckBox(UiStrings.KeepSession);
    private readonly CheckBox _singleInstance = C.CheckBox(UiStrings.SingleInstanceDesc);

    public GeneralSettingsPanel(IFormBase form, DesktopFormProvider desktopFormProvider) : base(form)
    {
        _desktopFormProvider = desktopFormProvider;
        UpdateValues(Config);
    }

    /// <summary>
    /// If set, a button for this command is shown in the interface section.
    /// </summary>
    public ActionCommand? KeyboardShortcutsCommand { get; set; }

    public override LayoutColumn BuildLayout()
    {
        return L.Column(
            L.GroupBox(
                UiStrings.Interface,
                L.Column(
                    PlatformCompat.System.SupportsTheme
                        ? L.Row(
                            C.Label(UiStrings.ThemeLabel).AlignCenter().Padding(right: 10),
                            _theme.AsControl().MinWidth(100)
                        )
                        : C.None(),
                    PlatformCompat.System.SupportsShowPageNumbers ? _showPageNumbers : C.None(),
                    PlatformCompat.System.SupportsProfilesToolbar ? _showProfilesToolbar : C.None(),
                    _scanChangesDefaultProfile,
                    PlatformCompat.System.SupportsButtonActions
                        ? L.Row(
                            C.Label(UiStrings.ScanButtonDefaultAction).AlignCenter().Padding(right: 20),
                            _scanButtonDefaultAction
                        ).Aligned()
                        : C.None(),
                    PlatformCompat.System.SupportsButtonActions
                        ? L.Row(
                            C.Label(UiStrings.SaveButtonDefaultAction).AlignCenter().Padding(right: 20),
                            _saveButtonDefaultAction
                        ).Aligned()
                        : C.None(),
                    PlatformCompat.System.SupportsKeyboardShortcuts && KeyboardShortcutsCommand != null
                        ? C.Button(KeyboardShortcutsCommand, ButtonImagePosition.Left).AlignLeading()
                        : C.None()
                )
            ),
            L.GroupBox(
                UiStrings.Application,
                L.Column(
                    _clearAfterSaving,
                    _keepSession,
                    PlatformCompat.System.SupportsSingleInstance
                        ? _singleInstance
                        : C.None()
                )
            ),
            C.Filler()
        );
    }

    private void UpdateValues(Naps2Config config)
    {
        void UpdateCheckbox(CheckBox checkBox, Expression<Func<CommonConfig, bool>> accessor)
        {
            checkBox.Checked = config.Get(accessor);
            checkBox.Enabled = !config.AppLocked.Has(accessor);
        }

        _theme.SelectedItem = config.Get(c => c.Theme);
        _theme.Enabled = !config.AppLocked.Has(c => c.Theme);
        UpdateCheckbox(_scanChangesDefaultProfile, c => c.ScanChangesDefaultProfile);
        UpdateCheckbox(_showProfilesToolbar, c => c.ShowProfilesToolbar);
        UpdateCheckbox(_showPageNumbers, c => c.ShowPageNumbers);
        _scanButtonDefaultAction.SelectedItem = config.Get(c => c.ScanButtonDefaultAction);
        _scanButtonDefaultAction.Enabled = !config.AppLocked.Has(c => c.ScanButtonDefaultAction);
        _saveButtonDefaultAction.SelectedItem = config.Get(c => c.SaveButtonDefaultAction);
        _saveButtonDefaultAction.Enabled = !config.AppLocked.Has(c => c.SaveButtonDefaultAction);
        UpdateCheckbox(_clearAfterSaving, c => c.DeleteAfterSaving);
        UpdateCheckbox(_keepSession, c => c.KeepSession);
        UpdateCheckbox(_singleInstance, c => c.SingleInstance);
    }

    public override void Save()
    {
        var transact = Config.User.BeginTransaction();
        bool SetIfChanged<T>(Expression<Func<CommonConfig, T>> accessor, T value)
        {
            var oldValue = Config.Get(accessor);
            if (!Equals(value, oldValue))
            {
                transact.Set(accessor, value);
                return true;
            }
            return false;
        }
        bool themeChanged = SetIfChanged(c => c.Theme, _theme.SelectedItem);
        SetIfChanged(c => c.ScanChangesDefaultProfile, _scanChangesDefaultProfile.IsChecked());
        SetIfChanged(c => c.ShowProfilesToolbar, _showProfilesToolbar.IsChecked());
        SetIfChanged(c => c.ShowPageNumbers, _showPageNumbers.IsChecked());
        SetIfChanged(c => c.ScanButtonDefaultAction, _scanButtonDefaultAction.SelectedItem);
        SetIfChanged(c => c.SaveButtonDefaultAction, _saveButtonDefaultAction.SelectedItem);
        SetIfChanged(c => c.DeleteAfterSaving, _clearAfterSaving.IsChecked());
        SetIfChanged(c => c.KeepSession, _keepSession.IsChecked());
        SetIfChanged(c => c.SingleInstance, _singleInstance.IsChecked());
        transact.Commit();

        _desktopFormProvider.DesktopForm.Invalidate();
        _desktopFormProvider.DesktopForm.PlaceProfilesToolbar();
        if (themeChanged)
        {
            EtoPlatform.Current.ColorScheme.UserThemeChanged();
        }
    }

    public override void RestoreDefaults()
    {
        UpdateValues(Config.DefaultsOnly);
    }
}