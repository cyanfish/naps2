using System.Linq.Expressions;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

internal class SettingsForm : EtoDialogBase
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
    private readonly CheckBox _apiEnableHttps = C.CheckBox("启用 API HTTPS");
    private readonly CheckBox _apiEnableCors = C.CheckBox("启用 API CORS");
    private readonly TextBox _apiHost = new();
    private readonly NumericUpDown _apiPort = new() { MinValue = 1024, MaxValue = 65535 };
    private readonly ActionCommand _pdfSettingsCommand;
    private readonly ActionCommand _imageSettingsCommand;
    private readonly ActionCommand _emailSettingsCommand;
    private readonly ActionCommand _keyboardShortcutsCommand;
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };

    public SettingsForm(Naps2Config config, DesktopSubFormController desktopSubFormController,
        DesktopFormProvider desktopFormProvider, IIconProvider iconProvider) : base(config)
    {
        Title = UiStrings.SettingsFormTitle;
        IconName = "cog_small";

        _desktopFormProvider = desktopFormProvider;
        UpdateValues(Config);
        _restoreDefaults.Click += RestoreDefaults_Click;

        _pdfSettingsCommand = new ActionCommand(desktopSubFormController.ShowPdfSettingsForm)
        {
            Text = UiStrings.PdfSettings,
            Image = iconProvider.GetIcon("file_extension_pdf_small")
        };
        _imageSettingsCommand = new ActionCommand(desktopSubFormController.ShowImageSettingsForm)
        {
            Text = UiStrings.ImageSettings,
            Image = iconProvider.GetIcon("picture_small")
        };
        _emailSettingsCommand = new ActionCommand(desktopSubFormController.ShowEmailSettingsForm)
        {
            Text = UiStrings.EmailSettings,
            Image = iconProvider.GetIcon("email_small")
        };
        _keyboardShortcutsCommand = new ActionCommand(() => FormFactory.Create<KeyboardShortcutsForm>().ShowModal())
        {
            Text = UiStrings.KeyboardShortcuts,
            Image = iconProvider.GetIcon("keyboard_small")
        };
    }

    protected override void BuildLayout()
    {
        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;

        var themeLabel = C.Label(UiStrings.ThemeLabel);
        themeLabel.TextAlignment = TextAlignment.Right;

        var scanButtonDefaultActionLabel = C.Label(UiStrings.ScanButtonDefaultAction);
        scanButtonDefaultActionLabel.TextAlignment = TextAlignment.Right;

        var saveButtonDefaultActionLabel = C.Label(UiStrings.SaveButtonDefaultAction);
        saveButtonDefaultActionLabel.TextAlignment = TextAlignment.Right;

        var interfacePage = new TabPage { Text = UiStrings.Interface };
        interfacePage.Content = new TableLayout
        {
            Padding = new Padding(10),
            Spacing = new Size(10, 10),
            Rows =
            {
                PlatformCompat.System.SupportsTheme
                    ? new TableRow(themeLabel, _theme.AsControl())
                    : new TableRow(),
                PlatformCompat.System.SupportsShowPageNumbers ? new TableRow(_showPageNumbers) : new TableRow(),
                PlatformCompat.System.SupportsProfilesToolbar ? new TableRow(_showProfilesToolbar) : new TableRow(),
                new TableRow(_scanChangesDefaultProfile),
                PlatformCompat.System.SupportsButtonActions
                    ? new TableRow(scanButtonDefaultActionLabel, _scanButtonDefaultAction.AsControl())
                    : new TableRow(),
                PlatformCompat.System.SupportsButtonActions
                    ? new TableRow(saveButtonDefaultActionLabel, _saveButtonDefaultAction.AsControl())
                    : new TableRow(),
                PlatformCompat.System.SupportsKeyboardShortcuts
                    ? new TableRow(C.Button(_keyboardShortcutsCommand, ButtonImagePosition.Left))
                    : new TableRow()
            }
        };

        var applicationPage = new TabPage { Text = UiStrings.Application };
        applicationPage.Content = new TableLayout
        {
            Padding = new Padding(10),
            Spacing = new Size(10, 10),
            Rows =
            {
                new TableRow(_clearAfterSaving),
                new TableRow(_keepSession),
                PlatformCompat.System.SupportsSingleInstance
                    ? new TableRow(_singleInstance)
                    : new TableRow()
            }
        };

        var apiHostLabel = C.Label("Host:");
        apiHostLabel.TextAlignment = TextAlignment.Right;

        var apiPortLabel = C.Label("端口:");
        apiPortLabel.TextAlignment = TextAlignment.Right;

        var apiPage = new TabPage { Text = "API 服务" };
        apiPage.Content = new TableLayout
        {
            Padding = new Padding(10),
            Spacing = new Size(10, 10),
            Rows =
            {
                new TableRow(apiHostLabel, _apiHost),
                new TableRow(apiPortLabel, _apiPort),
                new TableRow(_apiEnableHttps),
                new TableRow(_apiEnableCors)
            }
        };

        var tabControl = new TabControl { Pages = { interfacePage, applicationPage, apiPage } };

        LayoutController.Content = L.Column(
            tabControl,
            C.Filler(),
            L.Row(
                _restoreDefaults.MinWidth(140),
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Save),
                    C.CancelButton(this)
                )
            )
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
        _apiHost.Text = config.Get(c => c.ApiServerHost) ?? string.Empty;
        _apiPort.Value = config.Get(c => c.ApiServerPort);
        _apiEnableHttps.Checked = config.Get(c => c.ApiServerEnableHttps);
        _apiEnableCors.Checked = config.Get(c => c.ApiServerEnableCors);
    }

    private void Save()
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
        SetIfChanged(c => c.ApiServerHost, string.IsNullOrWhiteSpace(_apiHost.Text) ? "localhost" : _apiHost.Text.Trim());
        SetIfChanged(c => c.ApiServerPort, (int)_apiPort.Value);
        SetIfChanged(c => c.ApiServerEnableHttps, _apiEnableHttps.IsChecked());
        SetIfChanged(c => c.ApiServerEnableCors, _apiEnableCors.IsChecked());
        transact.Commit();

        _desktopFormProvider.DesktopForm.Invalidate();
        _desktopFormProvider.DesktopForm.PlaceProfilesToolbar();
        if (themeChanged)
        {
            EtoPlatform.Current.ColorScheme.UserThemeChanged();
        }
    }

    private void RestoreDefaults_Click(object? sender, EventArgs e)
    {
        UpdateValues(Config.DefaultsOnly);
    }
}