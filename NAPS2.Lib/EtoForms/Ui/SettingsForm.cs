using System.Linq.Expressions;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

internal class SettingsForm : EtoDialogBase
{
    private readonly DesktopFormProvider _desktopFormProvider;
    private readonly CheckBox _scanChangesDefaultProfile = C.CheckBox(UiStrings.ScanChangesDefaultProfile);
    private readonly CheckBox _showProfilesToolbar = C.CheckBox(UiStrings.ShowProfilesToolbar);
    private readonly CheckBox _showPageNumbers = C.CheckBox(UiStrings.ShowPageNumbers);
    private readonly DropDown _scanButtonDefaultAction = C.EnumDropDown<ScanButtonDefaultAction>();
    private readonly DropDown _saveButtonDefaultAction = C.EnumDropDown<SaveButtonDefaultAction>();
    private readonly CheckBox _clearAfterSaving = C.CheckBox(UiStrings.ClearAfterSaving);
    private readonly CheckBox _singleInstance = C.CheckBox(UiStrings.SingleInstanceDesc);
    private readonly Command _pdfSettingsCommand;
    private readonly Command _imageSettingsCommand;
    private readonly Command _emailSettingsCommand;
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };

    public SettingsForm(Naps2Config config, DesktopSubFormController desktopSubFormController,
        DesktopFormProvider desktopFormProvider) : base(config)
    {
        _desktopFormProvider = desktopFormProvider;
        UpdateValues(Config);
        _restoreDefaults.Click += RestoreDefaults_Click;

        _pdfSettingsCommand = new ActionCommand(desktopSubFormController.ShowPdfSettingsForm)
        {
            Text = UiStrings.PdfSettings,
            Image = Icons.file_extension_pdf_small.ToEtoImage()
        };
        _imageSettingsCommand = new ActionCommand(desktopSubFormController.ShowImageSettingsForm)
        {
            Text = UiStrings.ImageSettings,
            // TODO: Get an actual 16x16 image
            Image = Icons.picture_small.ToEtoImage()
        };
        _emailSettingsCommand = new ActionCommand(desktopSubFormController.ShowEmailSettingsForm)
        {
            Text = UiStrings.EmailSettings,
            Image = Icons.email_small.ToEtoImage()
        };
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.SettingsFormTitle;
        Icon = new Icon(1f, Icons.cog_small.ToEtoImage());

        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            L.GroupBox(
                UiStrings.Interface,
                L.Column(
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
                    _clearAfterSaving
                )
            ),
            PlatformCompat.System.SupportsSingleInstance
                ? L.GroupBox(
                    UiStrings.Application,
                    L.Column(
                        _singleInstance
                    )
                )
                : C.None(),
            // TODO: Probably only show these after we start adding tabs
            // L.Row(
            //     C.Button(_pdfSettingsCommand, ButtonImagePosition.Left),
            //     C.Button(_imageSettingsCommand, ButtonImagePosition.Left),
            //     C.Button(_emailSettingsCommand, ButtonImagePosition.Left)),
            C.Filler(),
            L.Row(
                _restoreDefaults.MinWidth(140),
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Save),
                    C.CancelButton(this))
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

        UpdateCheckbox(_scanChangesDefaultProfile, c => c.ScanChangesDefaultProfile);
        UpdateCheckbox(_showProfilesToolbar, c => c.ShowProfilesToolbar);
        UpdateCheckbox(_showPageNumbers, c => c.ShowPageNumbers);
        _scanButtonDefaultAction.SelectedIndex = (int) config.Get(c => c.ScanButtonDefaultAction);
        _scanButtonDefaultAction.Enabled = !config.AppLocked.Has(c => c.ScanButtonDefaultAction);
        _saveButtonDefaultAction.SelectedIndex = (int) config.Get(c => c.SaveButtonDefaultAction);
        _saveButtonDefaultAction.Enabled = !config.AppLocked.Has(c => c.SaveButtonDefaultAction);
        UpdateCheckbox(_clearAfterSaving, c => c.DeleteAfterSaving);
        UpdateCheckbox(_singleInstance, c => c.SingleInstance);
    }

    private void Save()
    {
        var transact = Config.User.BeginTransaction();
        void SetIfChanged<T>(Expression<Func<CommonConfig, T>> accessor, T value)
        {
            var oldValue = Config.Get(accessor);
            if (!Equals(value, oldValue))
            {
                transact.Set(accessor, value);
            }
        }
        SetIfChanged(c => c.ScanChangesDefaultProfile, _scanChangesDefaultProfile.IsChecked());
        SetIfChanged(c => c.ShowProfilesToolbar, _showProfilesToolbar.IsChecked());
        SetIfChanged(c => c.ShowPageNumbers, _showPageNumbers.IsChecked());
        SetIfChanged(c => c.ScanButtonDefaultAction, (ScanButtonDefaultAction) _scanButtonDefaultAction.SelectedIndex);
        SetIfChanged(c => c.SaveButtonDefaultAction, (SaveButtonDefaultAction) _saveButtonDefaultAction.SelectedIndex);
        SetIfChanged(c => c.DeleteAfterSaving, _clearAfterSaving.IsChecked());
        SetIfChanged(c => c.SingleInstance, _singleInstance.IsChecked());
        transact.Commit();

        _desktopFormProvider.DesktopForm.Invalidate();
        _desktopFormProvider.DesktopForm.PlaceProfilesToolbar();
    }

    private void RestoreDefaults_Click(object? sender, EventArgs e)
    {
        UpdateValues(Config.DefaultsOnly);
    }
}