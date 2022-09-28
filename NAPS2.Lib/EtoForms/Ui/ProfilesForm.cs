using Eto.Drawing;
using Eto.Forms;
using NAPS2.ImportExport.Profiles;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.EtoForms.Ui;

public class ProfilesForm : EtoDialogBase
{
    private readonly IScanPerformer _scanPerformer;
    private readonly ProfileNameTracker _profileNameTracker;
    private readonly IProfileManager _profileManager;
    private readonly ProfileTransfer _profileTransfer;
    private readonly ThumbnailController _thumbnailController;

    private readonly IListView<ScanProfile> _listView;

    private readonly Command _scanCommand;
    private readonly Command _addCommand;
    private readonly Command _editCommand;
    private readonly Command _deleteCommand;
    private readonly Command _setDefaultCommand;
    private readonly Command _copyCommand;
    private readonly Command _pasteCommand;

    public ProfilesForm(Naps2Config config, IScanPerformer scanPerformer, ProfileNameTracker profileNameTracker,
        IProfileManager profileManager, ProfileListViewBehavior profileListViewBehavior,
        ProfileTransfer profileTransfer, ThumbnailController thumbnailController)
        : base(config)
    {
        _scanPerformer = scanPerformer;
        _profileNameTracker = profileNameTracker;
        _profileManager = profileManager;
        _profileTransfer = profileTransfer;
        _thumbnailController = thumbnailController;

        Title = UiStrings.ProfilesFormTitle;
        Icon = new Icon(1f, Icons.blueprints_small.ToEtoImage());
        Size = new Size(700, 200);
        MinimumSize = new Size(600, 180);
        Resizable = true;

        // TODO: Do this only in WinForms (?)
        // switch (Handler)
        // {
        //     case IWindowsControl windowsControl:
        //         windowsControl.UseShellDropManager = false;
        //         break;
        // }

        _listView = EtoPlatform.Current.CreateListView(profileListViewBehavior);
        _scanCommand = new ActionCommand(DoScan)
        {
            MenuText = UiStrings.Scan,
            Image = Icons.control_play_blue_small.ToEtoImage(),
        };
        _addCommand = new ActionCommand(DoAdd)
        {
            MenuText = UiStrings.Add,
            Image = Icons.add_small.ToEtoImage()
        };
        _editCommand = new ActionCommand(DoEdit)
        {
            MenuText = UiStrings.Edit,
            Image = Icons.pencil_small.ToEtoImage()
        };
        _deleteCommand = new ActionCommand(DoDelete)
        {
            MenuText = UiStrings.Delete,
            Image = Icons.cross_small.ToEtoImage(),
            Shortcut = Keys.Delete
        };
        _setDefaultCommand = new ActionCommand(DoSetDefault)
        {
            MenuText = UiStrings.SetDefault,
            Image = Icons.accept_small.ToEtoImage()
        };
        _copyCommand = new ActionCommand(DoCopy)
        {
            MenuText = UiStrings.Copy,
            Shortcut = Keys.Control | Keys.C
        };
        _pasteCommand = new ActionCommand(DoPaste)
        {
            MenuText = UiStrings.Paste,
            Shortcut = Keys.Control | Keys.V
        };

        _listView.ImageSize = 48;
        _listView.AllowDrag = true;
        _listView.AllowDrop = !NoUserProfiles;
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

        ContextMenu = new ContextMenu();
        ContextMenu.AddItems(
            new ButtonMenuItem(_scanCommand),
            new ButtonMenuItem(_editCommand),
            new ButtonMenuItem(_setDefaultCommand),
            new SeparatorMenuItem());
        if (!NoUserProfiles)
        {
            ContextMenu.AddItems(
                new ButtonMenuItem(_copyCommand),
                new ButtonMenuItem(_pasteCommand),
                new SeparatorMenuItem());
        }
        ContextMenu.AddItems(
            new ButtonMenuItem(_deleteCommand));
        ContextMenu.Opening += ContextMenuOpening;

        BuildLayout();
    }

    private void BuildLayout()
    {
        Content = L.Column(
            L.Row(
                _listView.Control.XScale(),
                C.Button(_scanCommand, Icons.control_play_blue.ToEtoImage(), ButtonImagePosition.Above).AutoSize()
                    .Height(100)
            ).Aligned().YScale(),
            L.Row(
                L.Column(
                    L.Row(
                        C.Button(_addCommand, ButtonImagePosition.Left),
                        C.Button(_editCommand, ButtonImagePosition.Left),
                        C.Button(_deleteCommand, ButtonImagePosition.Left),
                        C.ZeroSpace()
                    )
                ),
                C.Button(UiStrings.Done, Close)
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
        if (_profileTransfer.IsIn(e.Data))
        {
            var data = _profileTransfer.GetFrom(e.Data);
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
                        new ListMutation<ScanProfile>.Append(data.ScanProfileXml.FromXml<ScanProfile>()), _listView);
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
            editSettingsForm.ScanProfile = new ScanProfile
            {
                Version = ScanProfile.CURRENT_VERSION
            };
            editSettingsForm.ShowModal();
            if (!editSettingsForm.Result)
            {
                return;
            }
            _profileManager.Mutate(new ListMutation<ScanProfile>.Append(editSettingsForm.ScanProfile),
                ListSelection.Empty<ScanProfile>());
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
        fedit.ScanProfile = Config.DefaultProfileSettings();
        fedit.ShowModal();
        if (fedit.Result)
        {
            _profileManager.Mutate(new ListMutation<ScanProfile>.Append(fedit.ScanProfile), _listView);
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
            if (MessageBox.Show(message, MiscResources.Delete, MessageBoxButtons.YesNo, MessageBoxType.Warning) ==
                DialogResult.Yes)
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
            _profileManager.Mutate(new ListMutation<ScanProfile>.Append(profile), _listView);
        }
    }
}