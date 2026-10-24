using Eto.Forms;
using NAPS2.Config.Model;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

internal class ToolbarSettingsPanel : SettingsPanel
{
    private readonly DesktopFormProvider _desktopFormProvider;
    private readonly EnumDropDownWidget<DockStyle> _mainPosition = new(scale: false);
    private readonly EnumDropDownWidget<DockStyle> _profilesPosition = new(scale: false);
    private readonly CheckBox _showProfilesToolbar = C.CheckBox(UiStrings.ShowProfilesToolbar);
    private readonly List<CheckBox> _allCheckBoxes;
    // The checkboxes that can be changed, i.e. excluding Settings
    private readonly List<(ToolbarButtons button, CheckBox checkBox)> _buttonCheckBoxes;
    // Updating the preview is relatively slow, so it's done on a timer to let the UI update first, and to batch
    // multiple changes
    private readonly UITimer _previewTimer = new() { Interval = 0.01 };
    private bool _updatingValues;
    private bool _previewing;

    public ToolbarSettingsPanel(IFormBase form, DesktopFormProvider desktopFormProvider) : base(form)
    {
        _desktopFormProvider = desktopFormProvider;
        var buttons = GetButtons().Select(x => (x.button, checkBox: C.CheckBox(x.label))).ToList();
        _allCheckBoxes = buttons.Select(x => x.checkBox).ToList();
        // Settings can't be removed (except by an admin with HideSettingsButton) so people don't accidentally lock
        // themselves out of toolbar settings
        var settingsCheckBox = buttons.Single(x => x.button == ToolbarButtons.Settings).checkBox;
        settingsCheckBox.Checked = Config.IsToolbarButtonShown(ToolbarButtons.Settings);
        settingsCheckBox.Enabled = false;
        _buttonCheckBoxes = buttons.Where(x => x.button != ToolbarButtons.Settings).ToList();
        UpdateValues(Config);
        _previewTimer.Elapsed += (_, _) => ApplyPreview();
        _mainPosition.SelectedItemChanged += (_, _) => UpdatePreview();
        _profilesPosition.SelectedItemChanged += (_, _) => UpdatePreview();
        _showProfilesToolbar.CheckedChanged += (_, _) =>
        {
            UpdateEnabled();
            UpdatePreview();
        };
        foreach (var (_, checkBox) in _buttonCheckBoxes)
        {
            checkBox.CheckedChanged += (_, _) => UpdatePreview();
        }
    }

    public override string Title => UiStrings.ToolbarSettings;

    public override string IconName => "application";

    // The buttons in the order they appear in the toolbar
    private static IEnumerable<(ToolbarButtons button, string label)> GetButtons()
    {
        yield return (ToolbarButtons.Scan, UiStrings.Scan);
        yield return (ToolbarButtons.Profiles, UiStrings.Profiles);
        yield return (ToolbarButtons.Ocr, UiStrings.Ocr);
        yield return (ToolbarButtons.Import, UiStrings.Import);
        yield return (ToolbarButtons.SavePdf, UiStrings.SavePdf);
        yield return (ToolbarButtons.SaveImages, UiStrings.SaveImages);
        if (PlatformCompat.System.CanEmail)
            yield return (ToolbarButtons.EmailPdf, UiStrings.EmailPdf);
        if (PlatformCompat.System.CanPrint)
            yield return (ToolbarButtons.Print, UiStrings.Print);
        yield return (ToolbarButtons.Image, UiStrings.Image);
        yield return (ToolbarButtons.Rotate, UiStrings.Rotate);
        yield return (ToolbarButtons.Move, UiStrings.MoveUpDown);
        yield return (ToolbarButtons.Reorder, UiStrings.Reorder);
        yield return (ToolbarButtons.Delete, UiStrings.Delete);
        yield return (ToolbarButtons.Clear, UiStrings.Clear);
        yield return (ToolbarButtons.Language, UiStrings.Language);
        yield return (ToolbarButtons.Settings, UiStrings.Settings);
        yield return (ToolbarButtons.About, UiStrings.About);
    }

    public override LayoutElement BuildScrollableLayout()
    {
        int half = (_allCheckBoxes.Count + 1) / 2;
        return L.Column(
            L.GroupBox(
                UiStrings.Toolbar,
                L.Column(
                    L.Row(
                        C.Label(UiStrings.PositionLabel).AlignCenter().Padding(right: 10),
                        _mainPosition.AsControl().MinWidth(100)
                    ).SpacingAfter(6),
                    C.Label(UiStrings.ButtonsLabel).SpacingAfter(6),
                    L.Row(
                        L.Column(_allCheckBoxes.Take(half).Expand()).Scale(),
                        L.Column(_allCheckBoxes.Skip(half).Expand()).Scale()
                    )
                )
            ),
            PlatformCompat.System.SupportsProfilesToolbar
                ? L.GroupBox(
                    UiStrings.ProfilesToolbar,
                    L.Column(
                        _showProfilesToolbar,
                        L.Row(
                            C.Label(UiStrings.PositionLabel).AlignCenter().Padding(right: 10),
                            _profilesPosition.AsControl().MinWidth(100)
                        )
                    )
                )
                : C.None()
        );
    }

    private void UpdateValues(Naps2Config config)
    {
        _updatingValues = true;
        _mainPosition.SelectedItem = config.Get(c => c.DesktopToolStripDock);
        _profilesPosition.SelectedItem = config.Get(c => c.ProfilesToolStripDock);
        _showProfilesToolbar.Checked = config.Get(c => c.ShowProfilesToolbar);
        foreach (var (button, checkBox) in _buttonCheckBoxes)
        {
            checkBox.Checked = config.IsToolbarButtonShown(button);
        }
        UpdateEnabled();
        _updatingValues = false;
    }

    private void UpdateEnabled()
    {
        _mainPosition.Enabled = !Config.AppLocked.Has(c => c.DesktopToolStripDock);
        _profilesPosition.Enabled = !Config.AppLocked.Has(c => c.ProfilesToolStripDock) &&
                                    _showProfilesToolbar.IsChecked();
        _showProfilesToolbar.Enabled = !Config.AppLocked.Has(c => c.ShowProfilesToolbar);
        foreach (var (button, checkBox) in _buttonCheckBoxes)
        {
            checkBox.Enabled = button.IsOptional()
                ? !Config.AppLocked.Has(c => c.ExtraButtons)
                : !Config.AppLocked.Has(c => c.HiddenButtons);
        }
    }

    private TransactionConfigScope<CommonConfig> BeginTransaction(out bool changed)
    {
        // Only update the flags for buttons listed here, so other flags (e.g. Sidebar) are preserved
        var hiddenButtons = Config.Get(c => c.HiddenButtons);
        var extraButtons = Config.Get(c => c.ExtraButtons);
        foreach (var (button, checkBox) in _buttonCheckBoxes)
        {
            if (button.IsOptional())
            {
                extraButtons = checkBox.IsChecked() ? extraButtons | button : extraButtons & ~button;
            }
            else
            {
                hiddenButtons = checkBox.IsChecked() ? hiddenButtons & ~button : hiddenButtons | button;
            }
        }

        var transact = Config.User.BeginTransaction();
        changed = false;
        changed |= transact.SetIfChanged(Config, c => c.DesktopToolStripDock, _mainPosition.SelectedItem);
        changed |= transact.SetIfChanged(Config, c => c.ProfilesToolStripDock, _profilesPosition.SelectedItem);
        changed |= transact.SetIfChanged(Config, c => c.ShowProfilesToolbar, _showProfilesToolbar.IsChecked());
        changed |= transact.SetIfChanged(Config, c => c.HiddenButtons, hiddenButtons);
        changed |= transact.SetIfChanged(Config, c => c.ExtraButtons, extraButtons);
        return transact;
    }

    // Show the uncommitted settings on the desktop form so the user can see what they look like
    private void UpdatePreview()
    {
        if (_updatingValues) return;
        _previewTimer.Start();
    }

    private void ApplyPreview()
    {
        _previewTimer.Stop();
        var transact = BeginTransaction(out _);
        _desktopFormProvider.DesktopForm.PreviewToolbarSettings(Config.WithTransaction(transact));
        _previewing = true;
    }

    private void EndPreview()
    {
        _previewTimer.Stop();
        if (_previewing)
        {
            _desktopFormProvider.DesktopForm.PreviewToolbarSettings(null);
            _previewing = false;
        }
    }

    public override void Save()
    {
        _previewTimer.Stop();
        var transact = BeginTransaction(out bool changed);
        transact.Commit();

        if (_previewing)
        {
            EndPreview();
        }
        else if (changed)
        {
            _desktopFormProvider.DesktopForm.ApplyToolbarSettings();
        }
    }

    public override void Cancel()
    {
        EndPreview();
    }

    public override void RestoreDefaults()
    {
        UpdateValues(Config.DefaultsOnly);
        UpdatePreview();
    }
}
