using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

internal class SettingsForm : EtoDialogBase
{
    private readonly CheckBox _scanChangesDefaultProfile = C.CheckBox(UiStrings.ScanChangesDefaultProfile);
    private readonly CheckBox _showProfilesToolbar = C.CheckBox(UiStrings.ShowProfilesToolbar);
    private readonly CheckBox _showPageNumbers = C.CheckBox(UiStrings.ShowPageNumbers);
    private readonly DropDown _saveButtonDefaultAction = C.EnumDropDown<SaveButtonDefaultAction>();
    private readonly CheckBox _clearAfterSaving = C.CheckBox(UiStrings.ClearAfterSaving);
    private readonly CheckBox _singleInstance = C.CheckBox(UiStrings.SingleInstanceDesc);
    private Command _pdfSettingsCommand;
    private Command _imageSettingsCommand;
    private Command _emailSettingsCommand;
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };

    public SettingsForm(Naps2Config config, DesktopSubFormController desktopSubFormController) : base(config)
    {
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

        // TODO: Customize settings per platform (incl. only 2 options for save action on Mac)
        // TODO: Disable locked settings
        LayoutController.Content = L.Column(
            L.GroupBox(
                UiStrings.Interface,
                L.Column(
                    _showPageNumbers,
                    _showProfilesToolbar,
                    _scanChangesDefaultProfile,
                    L.Row(
                        C.Label(UiStrings.SaveButtonDefaultAction).AlignCenter().Padding(right: 20),
                        _saveButtonDefaultAction
                    ),
                    _clearAfterSaving
                )
            ),
            L.GroupBox(
                UiStrings.Application,
                L.Column(
                    _singleInstance
                )
            ),
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
        _scanChangesDefaultProfile.Checked = config.Get(c => c.ScanMenuChangesDefaultProfile);
        _showProfilesToolbar.Checked = config.Get(c => c.ShowProfilesToolbar);
        _showPageNumbers.Checked = config.Get(c => c.ShowPageNumbers);
        _saveButtonDefaultAction.SelectedIndex = (int) config.Get(c => c.SaveButtonDefaultAction);
        _clearAfterSaving.Checked = config.Get(c => c.DeleteAfterSaving);
        _singleInstance.Checked = config.Get(c => c.SingleInstance);
    }

    private void Save()
    {
        // TODO: Maybe only save settings that have been user-changed
        var transact = Config.User.BeginTransaction();
        transact.Set(c => c.ScanMenuChangesDefaultProfile, _scanChangesDefaultProfile.IsChecked());
        transact.Set(c => c.ShowProfilesToolbar, _showProfilesToolbar.IsChecked());
        transact.Set(c => c.ShowPageNumbers, _showPageNumbers.IsChecked());
        transact.Set(c => c.SaveButtonDefaultAction, (SaveButtonDefaultAction) _saveButtonDefaultAction.SelectedIndex);
        transact.Set(c => c.DeleteAfterSaving, _clearAfterSaving.IsChecked());
        transact.Set(c => c.SingleInstance, _singleInstance.IsChecked());
        transact.Commit();
    }

    private void RestoreDefaults_Click(object? sender, EventArgs e)
    {
        UpdateValues(Config.DefaultsOnly);
    }
}