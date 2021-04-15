using System;
using System.Diagnostics;
using System.Linq;
using Eto.Forms;
using Eto.WinForms;
using Google.Protobuf;
using NAPS2.Config;
using NAPS2.Images;
using NAPS2.ImportExport;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Scan;
using NAPS2.Serialization;
using NAPS2.WinForms;

namespace NAPS2.EtoForms.Ui
{
    public class ProfilesForm : EtoDialogBase
    {
        private const int DEFAULT_PROFILE_ICON_ID = 3;
        private const int LOCK_PROFILE_ICON_ID = 4;
        private const int DEFAULT_LOCK_PROFILE_ICON_ID = 5;

        private readonly IScanPerformer _scanPerformer;
        private readonly ProfileNameTracker _profileNameTracker;
        private readonly IProfileManager _profileManager;
        private readonly SelectableListView<ScanProfile> _selectableListView;

        private readonly System.Windows.Forms.ListView _listView;
        
        private readonly Command _scanCommand;
        private readonly Command _addCommand;
        private readonly Command _editCommand;
        private readonly Command _deleteCommand;
        private readonly Command _setDefaultCommand;
        private readonly Command _copyCommand;
        private readonly Command _pasteCommand;

        public ProfilesForm(IScanPerformer scanPerformer, ProfileNameTracker profileNameTracker, IProfileManager profileManager)
        {
            _scanPerformer = scanPerformer;
            _profileNameTracker = profileNameTracker;
            _profileManager = profileManager;

            Title = UiStrings.ProfilesFormTitle;
            Icon = Icons.blueprints_small.ToEtoIcon();
            Resizable = true;

            _listView = new System.Windows.Forms.ListView
            {
                LargeImageList = new ILProfileIcons().IconsList,
                View = System.Windows.Forms.View.LargeIcon
            };
            _scanCommand = new ActionCommand(ScanClicked)
            {
                MenuText = UiStrings.Scan,
                Image = Icons.control_play_blue_small.ToEto(),
            };
            _addCommand = new ActionCommand(AddClicked)
            {
                MenuText = UiStrings.Add,
                Image = Icons.add_small.ToEto()
            };
            _editCommand = new ActionCommand(EditClicked)
            {
                MenuText = UiStrings.Edit,
                Image = Icons.pencil_small.ToEto()
            };
            _deleteCommand = new ActionCommand(DeleteClicked)
            {
                MenuText = UiStrings.Delete,
                Image = Icons.cross_small.ToEto()
            };
            _setDefaultCommand = new ActionCommand(SetDefaultClicked)
            {
                MenuText = UiStrings.SetDefault,
                Image = Icons.accept_small.ToEto()
            };
            _copyCommand = new ActionCommand(CopyClicked)
            {
                MenuText = UiStrings.Copy,
                Shortcut = Keys.Control | Keys.C
            };
            _pasteCommand = new ActionCommand(PasteClicked)
            {
                MenuText = UiStrings.Paste,
                Shortcut = Keys.Control | Keys.V
            };

            _listView.ItemDrag += lvProfiles_ItemDrag;
            _listView.ItemActivate += lvProfiles_ItemActivate;
            _listView.DragDrop += lvProfiles_DragDrop;
            _listView.DragEnter += lvProfiles_DragEnter;
            _listView.DragLeave += lvProfiles_DragLeave;
            _listView.DragOver += lvProfiles_DragOver;
            _listView.KeyDown += lvProfiles_KeyDown;
            _listView.AllowDrop = true;
            _selectableListView = new SelectableListView<ScanProfile>(_listView);
            _selectableListView.SelectionChanged += SelectionChanged;
            profileManager.ProfilesUpdated += ProfilesUpdated;
            
            _addCommand.Enabled = !NoUserProfiles;
            _editCommand.Enabled = false;
            _deleteCommand.Enabled = false;
            ReloadProfiles();
            var defaultProfile = _profileManager.Profiles.FirstOrDefault(x => x.IsDefault);
            if (defaultProfile != null)
            {
                _selectableListView.Selection = ListSelection.Of(defaultProfile);
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
                    _listView.ToEto().XScale(),
                    C.Button(_scanCommand, Icons.control_play_blue.ToEto(), ButtonImagePosition.Above).AutoSize()
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

        public Action<ScannedImage> ImageCallback { get; set; }

        private ScanProfile? SelectedProfile
        {
            get
            {
                if (_selectableListView.Selection.Count == 1)
                {
                    return _selectableListView.Selection.Single();
                }
                return null;
            }
        }

        private bool SelectionLocked
        {
            get { return _selectableListView.Selection.Any(x => x.IsLocked); }
        }

        private bool NoUserProfiles => false;//ConfigProvider.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked);

        private void ProfilesUpdated(object sender, EventArgs e)
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
            _selectableListView.RefreshItems(
                _profileManager.Profiles,
                profile => profile.DisplayName,
                profile =>
                    profile.IsDefault
                        ? profile.IsLocked ? DEFAULT_LOCK_PROFILE_ICON_ID : DEFAULT_PROFILE_ICON_ID
                        : profile.IsLocked
                            ? LOCK_PROFILE_ICON_ID
                            : profile.IconID);
        }

        private void AddClicked()
        {
            var fedit = FormFactory.Create<FEditProfile>();
            fedit.ScanProfile = ConfigProvider.Get(c => c.DefaultProfileSettings);
            fedit.ShowDialog();
            if (fedit.Result)
            {
                _profileManager.Mutate(new ListMutation<ScanProfile>.Append(fedit.ScanProfile), _selectableListView);
            }
        }

        private void EditClicked()
        {
            var originalProfile = SelectedProfile;
            if (originalProfile != null)
            {
                var fedit = FormFactory.Create<FEditProfile>();
                fedit.ScanProfile = originalProfile;
                fedit.ShowDialog();
                if (fedit.Result)
                {
                    _profileManager.Mutate(new ListMutation<ScanProfile>.ReplaceWith(fedit.ScanProfile), _selectableListView);
                }
            }
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            _editCommand.Enabled = _selectableListView.Selection.Count == 1;
            _deleteCommand.Enabled = _selectableListView.Selection.Count > 0 && !SelectionLocked;
        }

        private void DeleteClicked()
        {
            int selectedCount = _selectableListView.Selection.Count;
            if (selectedCount > 0 && !SelectionLocked)
            {
                string message = selectedCount == 1
                    ? string.Format(MiscResources.ConfirmDeleteSingleProfile, SelectedProfile.DisplayName)
                    : string.Format(MiscResources.ConfirmDeleteMultipleProfiles, selectedCount);
                if (MessageBox.Show(message, MiscResources.Delete, MessageBoxButtons.YesNo, MessageBoxType.Warning) == DialogResult.Yes)
                {
                    foreach (var profile in _selectableListView.Selection)
                    {
                        _profileNameTracker.DeletingProfile(profile.DisplayName);
                    }
                    _profileManager.Mutate(new ListMutation<ScanProfile>.DeleteSelected(), _selectableListView);
                }
            }
        }

        private void lvProfiles_ItemActivate(object sender, EventArgs e)
        {
            if (SelectedProfile != null)
            {
                ScanClicked();
            }
        }

        private void lvProfiles_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyData == System.Windows.Forms.Keys.Delete && _deleteCommand.Enabled)
            {
                DeleteClicked();
            }
        }

        private ScanParams DefaultScanParams() =>
            new ScanParams
            {
                NoAutoSave = ConfigProvider.Get(c => c.DisableAutoSave),
                DoOcr = ConfigProvider.Get(c => c.EnableOcr) && ConfigProvider.Get(c => c.OcrAfterScanning),
                ThumbnailSize = ConfigProvider.Get(c => c.ThumbnailSize)
            };

        private async void ScanClicked()
        {
            if (_profileManager.Profiles.Count == 0)
            {
                var editSettingsForm = FormFactory.Create<FEditProfile>();
                editSettingsForm.ScanProfile = new ScanProfile
                {
                    Version = ScanProfile.CURRENT_VERSION
                };
                editSettingsForm.ShowDialog();
                if (!editSettingsForm.Result)
                {
                    return;
                }
                _profileManager.Mutate(new ListMutation<ScanProfile>.Append(editSettingsForm.ScanProfile), ListSelection.Empty<ScanProfile>());
                _profileManager.DefaultProfile = editSettingsForm.ScanProfile;
            }
            if (SelectedProfile == null)
            {
                MessageBox.Show(MiscResources.SelectProfileBeforeScan, MiscResources.ChooseProfile, MessageBoxButtons.OK, MessageBoxType.Warning);
                return;
            }
            if (_profileManager.DefaultProfile == null)
            {
                _profileManager.DefaultProfile = SelectedProfile;
            }
            var source = await _scanPerformer.PerformScan(SelectedProfile, DefaultScanParams(), this.ToNative().Handle);
            await source.ForEach(ImageCallback);
            this.ToNative().Activate();
        }
        
        private void ContextMenuOpening(object sender, EventArgs e)
        {
            _setDefaultCommand.Enabled = SelectedProfile != null && !SelectedProfile.IsDefault;
            _editCommand.Enabled = SelectedProfile != null;
            _deleteCommand.Enabled = SelectedProfile != null && !SelectedProfile.IsLocked;
            _copyCommand.Enabled = SelectedProfile != null;
            _pasteCommand.Enabled = TransferHelper.ClipboardHasProfile();
        }

        private void SetDefaultClicked()
        {
            if (SelectedProfile != null)
            {
                _profileManager.DefaultProfile = SelectedProfile;
            }
        }

        private void CopyClicked()
        {
            TransferHelper.SaveProfileToClipboard(SelectedProfile);
        }

        private void PasteClicked()
        {
            if (NoUserProfiles)
            {
                return;
            }
            if (TransferHelper.ClipboardHasProfile())
            {
                var transfer = TransferHelper.GetProfileFromClipboard();
                var profile = transfer.ScanProfileXml.FromXml<ScanProfile>();
                _profileManager.Mutate(new ListMutation<ScanProfile>.Append(profile), _selectableListView);
            }
        }

        #region Drag/Drop
        
        private void lvProfiles_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
        {
            // Provide drag data
            if (_selectableListView.Selection.Count > 0)
            {
                var ido = new System.Windows.Forms.DataObject();
                ido.SetData(TransferHelper.ProfileTypeName, TransferHelper.Profile(SelectedProfile).ToByteArray());
                this.ToNative().DoDragDrop(ido, System.Windows.Forms.DragDropEffects.Move | System.Windows.Forms.DragDropEffects.Copy);
            }
        }
        
        private void lvProfiles_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            // Determine if drop data is compatible
            if (NoUserProfiles)
            {
                return;
            }
            try
            {
                if (e.Data.GetDataPresent(TransferHelper.ProfileTypeName))
                {
                    var data = DirectProfileTransfer.Parser.ParseFrom((byte[])e.Data.GetData(TransferHelper.ProfileTypeName));
                    e.Effect = data.ProcessId == Process.GetCurrentProcess().Id
                        ? data.Locked
                            ? System.Windows.Forms.DragDropEffects.None
                            : System.Windows.Forms.DragDropEffects.Move
                        : System.Windows.Forms.DragDropEffects.Copy;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error receiving drag/drop", ex);
            }
        }
        
        private void lvProfiles_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            // Receive drop data
            if (e.Data.GetDataPresent(TransferHelper.ProfileTypeName))
            {
                var data = DirectProfileTransfer.Parser.ParseFrom((byte[])e.Data.GetData(TransferHelper.ProfileTypeName));
                if (data.ProcessId == Process.GetCurrentProcess().Id)
                {
                    DragMoveProfile(e);
                }
                else
                {
                    if (!NoUserProfiles)
                    {
                        _profileManager.Mutate(new ListMutation<ScanProfile>.Append(data.ScanProfileXml.FromXml<ScanProfile>()), _selectableListView);
                    }
                }
            }
            _listView.InsertionMark.Index = -1;
        }
        
        private void lvProfiles_DragLeave(object sender, EventArgs e)
        {
            _listView.InsertionMark.Index = -1;
        }
        
        private void DragMoveProfile(System.Windows.Forms.DragEventArgs e)
        {
            var selectedProfile = SelectedProfile;
            if (selectedProfile == null)
            {
                return;
            }
            int index = GetDragIndex(e);
            if (index != -1)
            {
                if (selectedProfile.IsLocked)
                {
                    return;
                }
                while (index < _profileManager.Profiles.Count && _profileManager.Profiles[index].IsLocked)
                {
                    index++;
                }
                _profileManager.Mutate(new ListMutation<ScanProfile>.MoveTo(index), _selectableListView);
            }
        }
        
        private void lvProfiles_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Effect == System.Windows.Forms.DragDropEffects.Move)
            {
                var index = GetDragIndex(e);
                if (index == _profileManager.Profiles.Count)
                {
                    _listView.InsertionMark.Index = index - 1;
                    _listView.InsertionMark.AppearsAfterItem = true;
                }
                else
                {
                    _listView.InsertionMark.Index = index;
                    _listView.InsertionMark.AppearsAfterItem = false;
                }
            }
        }
        
        private int GetDragIndex(System.Windows.Forms.DragEventArgs e)
        {
            System.Drawing.Point cp = _listView.PointToClient(new System.Drawing.Point(e.X, e.Y));
            System.Windows.Forms.ListViewItem dragToItem = _listView.GetItemAt(cp.X, cp.Y);
            if (dragToItem == null)
            {
                var items = _listView.Items.Cast<System.Windows.Forms.ListViewItem>().ToList();
                var minY = items.Select(x => x.Bounds.Top).Min();
                var maxY = items.Select(x => x.Bounds.Bottom).Max();
                if (cp.Y < minY)
                {
                    cp.Y = minY;
                }
                if (cp.Y > maxY)
                {
                    cp.Y = maxY;
                }
                var row = items.Where(x => x.Bounds.Top <= cp.Y && x.Bounds.Bottom >= cp.Y).OrderBy(x => x.Bounds.X).ToList();
                dragToItem = row.FirstOrDefault(x => x.Bounds.Right >= cp.X) ?? row.LastOrDefault();
            }
            if (dragToItem == null)
            {
                return -1;
            }
            int dragToIndex = dragToItem.Index;
            if (cp.X > (dragToItem.Bounds.X + dragToItem.Bounds.Width / 2))
            {
                dragToIndex++;
            }
            return dragToIndex;
        }
        
        #endregion
    }
}