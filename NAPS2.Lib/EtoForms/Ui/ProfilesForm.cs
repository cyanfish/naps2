using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.ImportExport.Profiles;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.EtoForms.Ui;

public class ProfilesForm : EtoDialogBase
{
    private readonly IScanPerformer _scanPerformer;
    private readonly ProfileNameTracker _profileNameTracker;
    private readonly IProfileManager _profileManager;
    private readonly ThumbnailController _thumbnailController;
    private readonly IIconProvider _iconProvider;
    private readonly ProfileTransfer _profileTransfer = new();

    private readonly IListView<ScanProfile> _listView;

    private readonly Command _scanCommand;
    private readonly Command _addCommand;
    private readonly Command _editCommand;
    private readonly Command _deleteCommand;
    private readonly Command _setDefaultCommand;
    private readonly Command _copyCommand;
    private readonly Command _pasteCommand;
    private readonly Command _scannerSharingCommand;

    public ProfilesForm(Naps2Config config, IScanPerformer scanPerformer, ProfileNameTracker profileNameTracker,
        IProfileManager profileManager, ProfileListViewBehavior profileListViewBehavior,
        ThumbnailController thumbnailController, IIconProvider iconProvider)
        : base(config)
    {
        Title = UiStrings.ProfilesFormTitle;
        Icon = iconProvider.GetFormIcon("blueprints_small");

        _scanPerformer = scanPerformer;
        _profileNameTracker = profileNameTracker;
        _profileManager = profileManager;
        _thumbnailController = thumbnailController;
        _iconProvider = iconProvider;

        // TODO: Do this only in WinForms (?)
        // switch (Handler)
        // {
        //     case IWindowsControl windowsControl:
        //         windowsControl.UseShellDropManager = false;
        //         break;
        // }

        profileListViewBehavior.NoUserProfiles = NoUserProfiles;
        _listView = EtoPlatform.Current.CreateListView(profileListViewBehavior);
        _scanCommand = new ActionCommand(DoScan)
        {
            MenuText = UiStrings.Scan,
            Image = iconProvider.GetIcon("control_play_blue_small"),
        };
        _addCommand = new ActionCommand(DoAdd)
        {
            MenuText = UiStrings.New,
            Image = iconProvider.GetIcon("add_small")
        };
        _editCommand = new ActionCommand(DoEdit)
        {
            MenuText = UiStrings.Edit,
            Image = iconProvider.GetIcon("pencil_small")
        };
        _deleteCommand = new ActionCommand(DoDelete)
        {
            MenuText = UiStrings.Delete,
            Image = iconProvider.GetIcon("cross_small")
        };
        _setDefaultCommand = new ActionCommand(DoSetDefault)
        {
            MenuText = UiStrings.SetDefault,
            Image = iconProvider.GetIcon("accept_small")
        };
        _copyCommand = new ActionCommand(DoCopy)
        {
            MenuText = UiStrings.Copy
        };
        _pasteCommand = new ActionCommand(DoPaste)
        {
            MenuText = UiStrings.Paste
        };
        _scannerSharingCommand = new ActionCommand(OpenScannerSharingForm)
        {
            MenuText = UiStrings.ScannerSharing,
            Image = iconProvider.GetIcon("wireless16")
        };

        var profilesKsm = new KeyboardShortcutManager();
        profilesKsm.Assign("Esc", Close);
        profilesKsm.Assign("Del", _deleteCommand);
        profilesKsm.Assign("Mod+C", _copyCommand);
        profilesKsm.Assign("Mod+V", _pasteCommand);
        EtoPlatform.Current.HandleKeyDown(_listView.Control, profilesKsm.Perform);

        _listView.ImageSize = new Size(48, 48);
        _listView.ItemClicked += ItemClicked;
        _listView.SelectionChanged += SelectionChanged;
        _listView.Drop += Drop;
        profileManager.ProfilesUpdated += ProfilesUpdated;

        _addCommand.Enabled = !NoUserProfiles;
        _editCommand.Enabled = false;
        _deleteCommand.Enabled = false;
        ReloadProfiles();
        var defaultProfile = _profileManager.Profiles.FirstOrDefault(x => x.IsDefault);
        if (defaultProfile != null)
        {
            _listView.Selection = ListSelection.Of(defaultProfile);
        }

        var contextMenu = new ContextMenu();
        _listView.ContextMenu = contextMenu;
        contextMenu.AddItems(
            new ButtonMenuItem(_scanCommand),
            new ButtonMenuItem(_editCommand),
            new ButtonMenuItem(_setDefaultCommand),
            new SeparatorMenuItem());
        if (!NoUserProfiles)
        {
            contextMenu.AddItems(
                new ButtonMenuItem(_copyCommand),
                new ButtonMenuItem(_pasteCommand),
                new SeparatorMenuItem());
        }
        contextMenu.AddItems(
            new ButtonMenuItem(_deleteCommand));
        contextMenu.Opening += ContextMenuOpening;
    }

    protected override void BuildLayout()
    {
        FormStateController.DefaultExtraLayoutSize = new Size(200, 0);

        LayoutController.Content = L.Column(
            L.Row(
                _listView.Control.Scale(),
                C.Button(_scanCommand, _iconProvider.GetIcon("control_play_blue", true)!, ButtonImagePosition.Above)
                    .Height(80)
            ).Aligned().Scale(),
            L.Row(
                L.Column(
                    L.Row(
                        C.Button(_addCommand, ButtonImagePosition.Left),
                        C.Button(_editCommand, ButtonImagePosition.Left),
                        C.Button(_deleteCommand, ButtonImagePosition.Left),
                        C.Filler(),
                        Config.Get(c => c.DisableScannerSharing)
                            ? C.None()
                            : C.Button(_scannerSharingCommand, ButtonImagePosition.Left)
                    )
                ),
                C.CancelButton(this, UiStrings.Done)
            ).Aligned());
    }

    public Action<ProcessedImage>? ImageCallback { get; set; }

    private ScanProfile? SelectedProfile => _listView.Selection.SingleOrDefault();

    private bool SelectionLocked
    {
        get { return _listView.Selection.Any(x => x.IsLocked); }
    }

    private bool NoUserProfiles => Config.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked);

    private void ProfilesUpdated(object? sender, EventArgs e)
    {
        ReloadProfiles();

        // If we only have one profile, make it the default
        var profiles = _profileManager.Profiles;
        if (profiles.Count == 1 && !profiles[0].IsDefault)
        {
            _profileManager.DefaultProfile = profiles.Single();
        }
    }

    private void ReloadProfiles()
    {
        _listView.SetItems(_profileManager.Profiles);
    }

    private void SelectionChanged(object? sender, EventArgs e)
    {
        _editCommand.Enabled = _listView.Selection.Count == 1;
        _deleteCommand.Enabled = _listView.Selection.Count > 0 && !SelectionLocked;
    }

    private void ItemClicked(object? sender, EventArgs e)
    {
        if (SelectedProfile != null)
        {
            DoScan();
        }
    }

    private void Drop(object? sender, DropEventArgs e)
    {
        // Receive drop data
        if (e.CustomData != null)
        {
            var data = _profileTransfer.FromBinaryData(e.CustomData);
            if (data.ProcessId == Process.GetCurrentProcess().Id)
            {
                if (data.Locked)
                {
                    return;
                }
                int index = e.Position;
                while (index < _profileManager.Profiles.Count && _profileManager.Profiles[index].IsLocked)
                {
                    index++;
                }
                _profileManager.Mutate(new ListMutation<ScanProfile>.MoveTo(index), _listView);
            }
            else
            {
                if (!NoUserProfiles)
                {
                    _profileManager.Mutate(
                        new ListMutation<ScanProfile>.AppendAndSelect(data.ScanProfileXml.FromXml<ScanProfile>()),
                        _listView);
                }
            }
        }
    }

    private ScanParams DefaultScanParams() =>
        new()
        {
            NoAutoSave = Config.Get(c => c.DisableAutoSave),
            OcrParams = Config.OcrAfterScanningParams(),
            ThumbnailSize = _thumbnailController.RenderSize
        };

    private void ContextMenuOpening(object? sender, EventArgs e)
    {
        _setDefaultCommand.Enabled = SelectedProfile != null && !SelectedProfile.IsDefault;
        _editCommand.Enabled = SelectedProfile != null;
        _deleteCommand.Enabled = SelectedProfile != null && !SelectedProfile.IsLocked;
        _copyCommand.Enabled = SelectedProfile != null;
        _pasteCommand.Enabled = _profileTransfer.IsInClipboard();
    }

    private async void DoScan()
    {
        if (ImageCallback == null)
        {
            throw new InvalidOperationException("Image callback not specified");
        }
        if (_profileManager.Profiles.Count == 0)
        {
            var editSettingsForm = FormFactory.Create<EditProfileForm>();
            editSettingsForm.NewProfile = true;
            editSettingsForm.ScanProfile = new ScanProfile
            {
                Version = ScanProfile.CURRENT_VERSION
            };
            editSettingsForm.ShowModal();
            if (!editSettingsForm.Result)
            {
                return;
            }
            _profileManager.Mutate(new ListMutation<ScanProfile>.AppendAndSelect(editSettingsForm.ScanProfile),
                _listView);
            _profileManager.DefaultProfile = editSettingsForm.ScanProfile;
        }
        if (SelectedProfile == null)
        {
            MessageBox.Show(MiscResources.SelectProfileBeforeScan, MiscResources.ChooseProfile, MessageBoxButtons.OK,
                MessageBoxType.Warning);
            return;
        }
        if (_profileManager.DefaultProfile == null)
        {
            _profileManager.DefaultProfile = SelectedProfile;
        }
        var images = _scanPerformer.PerformScan(SelectedProfile, DefaultScanParams(), NativeHandle);
        await foreach (var image in images)
        {
            ImageCallback(image);
        }
        Focus();
    }

    private void DoAdd()
    {
        var fedit = FormFactory.Create<EditProfileForm>();
        fedit.NewProfile = true;
        fedit.ScanProfile = Config.DefaultProfileSettings();
        fedit.ShowModal();
        if (fedit.Result)
        {
            _profileManager.Mutate(new ListMutation<ScanProfile>.AppendAndSelect(fedit.ScanProfile), _listView);
        }
    }

    private void DoEdit()
    {
        var originalProfile = SelectedProfile;
        if (originalProfile != null)
        {
            var fedit = FormFactory.Create<EditProfileForm>();
            fedit.ScanProfile = originalProfile;
            fedit.ShowModal();
            if (fedit.Result)
            {
                _profileManager.Mutate(new ListMutation<ScanProfile>.ReplaceWith(fedit.ScanProfile), _listView);
            }
        }
    }

    private void DoDelete()
    {
        if (SelectedProfile != null && !SelectionLocked)
        {
            string message = string.Format(MiscResources.ConfirmDeleteSingleProfile, SelectedProfile.DisplayName);
            if (MessageBox.Show(message, MiscResources.Delete, MessageBoxButtons.OKCancel, MessageBoxType.Warning,
                    MessageBoxDefaultButton.OK) == DialogResult.Ok)
            {
                foreach (var profile in _listView.Selection)
                {
                    _profileNameTracker.DeletingProfile(profile.DisplayName);
                }
                _profileManager.Mutate(new ListMutation<ScanProfile>.DeleteSelected(), _listView);
            }
        }
    }

    private void DoSetDefault()
    {
        if (SelectedProfile != null)
        {
            _profileManager.DefaultProfile = SelectedProfile;
        }
    }

    private void DoCopy()
    {
        if (SelectedProfile != null)
        {
            _profileTransfer.SetClipboard(SelectedProfile);
        }
    }

    private void DoPaste()
    {
        if (NoUserProfiles)
        {
            return;
        }
        if (_profileTransfer.IsInClipboard())
        {
            var data = _profileTransfer.GetFromClipboard();
            var profile = data.ScanProfileXml.FromXml<ScanProfile>();
            _profileManager.Mutate(new ListMutation<ScanProfile>.AppendAndSelect(profile), _listView);
        }
    }

    private void OpenScannerSharingForm()
    {
        var form = FormFactory.Create<ScannerSharingForm>();
        form.ShowModal();
    }
}