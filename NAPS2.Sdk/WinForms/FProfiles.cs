using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Platform;
using NAPS2.Scan;
using NAPS2.Images;
using NAPS2.Serialization;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class FProfiles : FormBase
    {
        private const int DEFAULT_PROFILE_ICON_ID = 3;
        private const int LOCK_PROFILE_ICON_ID = 4;
        private const int DEFAULT_LOCK_PROFILE_ICON_ID = 5;

        private readonly IconButtonSizer iconButtonSizer;
        private readonly IScanPerformer scanPerformer;
        private readonly ProfileNameTracker profileNameTracker;
        private readonly IProfileManager profileManager;
        private readonly SelectableListView<ScanProfile> selectableListView;

        public FProfiles(IconButtonSizer iconButtonSizer, IScanPerformer scanPerformer, ProfileNameTracker profileNameTracker, IProfileManager profileManager)
        {
            this.iconButtonSizer = iconButtonSizer;
            this.scanPerformer = scanPerformer;
            this.profileNameTracker = profileNameTracker;
            this.profileManager = profileManager;
            InitializeComponent();

            selectableListView = new SelectableListView<ScanProfile>(lvProfiles);
            selectableListView.SelectionChanged += SelectionChanged;
            profileManager.ProfilesUpdated += ProfilesUpdated;
        }

        public Action<ScannedImage> ImageCallback { get; set; }

        private ScanProfile SelectedProfile
        {
            get
            {
                if (selectableListView.Selection.Count == 1)
                {
                    return selectableListView.Selection.Single();
                }
                return null;
            }
        }

        private bool SelectionLocked
        {
            get { return selectableListView.Selection.Any(x => x.IsLocked); }
        }

        private bool NoUserProfiles => ConfigProvider.Get(c => c.NoUserProfiles) && profileManager.Profiles.Any(x => x.IsLocked);

        protected override void OnLoad(object sender, EventArgs e)
        {
            lvProfiles.LargeImageList = ilProfileIcons.IconsList;
            btnAdd.Enabled = !NoUserProfiles;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            ReloadProfiles();
            var defaultProfile = profileManager.Profiles.FirstOrDefault(x => x.IsDefault);
            if (defaultProfile != null)
            {
                selectableListView.Selection = ListSelection.From(defaultProfile);
            }

            if (NoUserProfiles)
            {
                contextMenuStrip.Items.Remove(ctxCopy);
                contextMenuStrip.Items.Remove(ctxPaste);
                contextMenuStrip.Items.Remove(toolStripSeparator2);
            }

            if (!PlatformCompat.Runtime.IsImagePaddingSupported)
            {
                btnScan.ImageAlign = ContentAlignment.MiddleCenter;
            }

            var lm = new LayoutManager(this)
                .Bind(lvProfiles)
                .WidthToForm()
                .HeightToForm()
                .Bind(btnAdd, btnEdit, btnDelete, btnDone)
                .BottomToForm()
                .Bind(btnDone, btnScan)
                .RightToForm()
                .Bind(btnEdit)
                .LeftTo(() => btnAdd.Right)
                .Bind(btnDelete)
                .LeftTo(() => btnEdit.Right)
                .Activate();

            iconButtonSizer.WidthOffset = 20;
            iconButtonSizer.PaddingRight = 4;
            iconButtonSizer.MaxWidth = 100;
            iconButtonSizer.ResizeButtons(btnAdd, btnEdit, btnDelete);

            lm.UpdateLayout();
        }

        private void ProfilesUpdated(object sender, EventArgs e)
        {
            ReloadProfiles();
            
            // If we only have one profile, make it the default
            var profiles = profileManager.Profiles;
            if (profiles.Count == 1 && !profiles[0].IsDefault)
            {
                profileManager.DefaultProfile = profiles.Single();
            }
        }

        private void ReloadProfiles()
        {
            selectableListView.RefreshItems(
                profileManager.Profiles,
                profile => profile.DisplayName,
                profile =>
                    profile.IsDefault
                        ? profile.IsLocked ? DEFAULT_LOCK_PROFILE_ICON_ID : DEFAULT_PROFILE_ICON_ID
                        : profile.IsLocked
                            ? LOCK_PROFILE_ICON_ID
                            : profile.IconID);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var fedit = FormFactory.Create<FEditProfile>();
            fedit.ScanProfile = ConfigProvider.Get(c => c.DefaultProfileSettings);
            fedit.ShowDialog();
            if (fedit.Result)
            {
                profileManager.Mutate(new ListMutation<ScanProfile>.Append(fedit.ScanProfile), selectableListView);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var originalProfile = SelectedProfile;
            if (originalProfile != null)
            {
                var fedit = FormFactory.Create<FEditProfile>();
                fedit.ScanProfile = originalProfile;
                fedit.ShowDialog();
                if (fedit.Result)
                {
                    profileManager.Mutate(new ListMutation<ScanProfile>.ReplaceWith(fedit.ScanProfile), selectableListView);
                }
            }
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            btnEdit.Enabled = selectableListView.Selection.Count == 1;
            btnDelete.Enabled = selectableListView.Selection.Count > 0 && !SelectionLocked;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int selectedCount = selectableListView.Selection.Count;
            if (selectedCount > 0 && !SelectionLocked)
            {
                string message = selectedCount == 1
                    ? string.Format(MiscResources.ConfirmDeleteSingleProfile, SelectedProfile.DisplayName)
                    : string.Format(MiscResources.ConfirmDeleteMultipleProfiles, selectedCount);
                if (MessageBox.Show(message, MiscResources.Delete, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    foreach (var profile in selectableListView.Selection)
                    {
                        profileNameTracker.DeletingProfile(profile.DisplayName);
                    }
                    profileManager.Mutate(new ListMutation<ScanProfile>.DeleteSelected(), selectableListView);
                }
            }
        }

        private void lvProfiles_ItemActivate(object sender, EventArgs e)
        {
            if (SelectedProfile != null)
            {
                PerformScan();
            }
        }

        private void lvProfiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && btnDelete.Enabled)
            {
                btnDelete_Click(null, null);
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            PerformScan();
        }

        private ScanParams DefaultScanParams() =>
            new ScanParams
            {
                NoAutoSave = ConfigProvider.Get(c => c.DisableAutoSave),
                DoOcr = ConfigProvider.Get(c => c.EnableOcr) && ConfigProvider.Get(c => c.OcrAfterScanning),
                ThumbnailSize = ConfigProvider.Get(c => c.ThumbnailSize)
            };

        private async void PerformScan()
        {
            if (profileManager.Profiles.Count == 0)
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
                profileManager.Mutate(new ListMutation<ScanProfile>.Append(editSettingsForm.ScanProfile), ListSelection.Empty<ScanProfile>());
                profileManager.DefaultProfile = editSettingsForm.ScanProfile;
            }
            if (SelectedProfile == null)
            {
                MessageBox.Show(MiscResources.SelectProfileBeforeScan, MiscResources.ChooseProfile, MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (profileManager.DefaultProfile == null)
            {
                profileManager.DefaultProfile = SelectedProfile;
            }
            var source = await scanPerformer.PerformScan(SelectedProfile, DefaultScanParams(), Handle);
            await source.ForEach(ImageCallback);
            Activate();
        }

        private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var ido = Clipboard.GetDataObject();
            var canPaste = ido != null && ido.GetDataPresent(typeof(DirectProfileTransfer).FullName);
            if (SelectedProfile == null)
            {
                if (canPaste)
                {
                    foreach (ToolStripItem item in contextMenuStrip.Items)
                    {
                        if (item != ctxPaste)
                        {
                            item.Visible = false;
                        }
                    }
                    ctxPaste.Enabled = true;
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                foreach (ToolStripItem item in contextMenuStrip.Items)
                {
                    item.Visible = true;
                }
                ctxSetDefault.Enabled = !SelectedProfile.IsDefault;
                ctxEdit.Enabled = true;
                ctxDelete.Enabled = !SelectedProfile.IsLocked;
                ctxCopy.Enabled = true;
                ctxPaste.Enabled = canPaste;
            }
        }

        private void ctxScan_Click(object sender, EventArgs e)
        {
            PerformScan();
        }

        private void ctxEdit_Click(object sender, EventArgs e)
        {
            btnEdit_Click(null, null);
        }

        private void ctxDelete_Click(object sender, EventArgs e)
        {
            btnDelete_Click(null, null);
        }

        private void ctxSetDefault_Click(object sender, EventArgs e)
        {
            if (SelectedProfile != null)
            {
                profileManager.DefaultProfile = SelectedProfile;
            }
        }

        private IDataObject GetSelectedProfileDataObject()
        {
            IDataObject ido = new DataObject();
            ido.SetData(typeof(DirectProfileTransfer), new DirectProfileTransfer(SelectedProfile));
            return ido;
        }

        private void ctxCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(GetSelectedProfileDataObject());
        }

        private void ctxPaste_Click(object sender, EventArgs e)
        {
            if (NoUserProfiles)
            {
                return;
            }
            var ido = Clipboard.GetDataObject();
            if (ido == null)
            {
                return;
            }
            if (ido.GetDataPresent(typeof(DirectProfileTransfer).FullName))
            {
                var data = (DirectProfileTransfer) ido.GetData(typeof(DirectProfileTransfer).FullName);
                profileManager.Mutate(new ListMutation<ScanProfile>.Append(data.ScanProfileXml.FromXml<ScanProfile>()), selectableListView);
            }
        }

        #region Drag/Drop

        private void lvProfiles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Provide drag data
            if (selectableListView.Selection.Count > 0)
            {
                var ido = GetSelectedProfileDataObject();
                DoDragDrop(ido, DragDropEffects.Move | DragDropEffects.Copy);
            }
        }

        private void lvProfiles_DragEnter(object sender, DragEventArgs e)
        {
            // Determine if drop data is compatible
            if (NoUserProfiles)
            {
                return;
            }
            try
            {
                if (e.Data.GetDataPresent(typeof(DirectProfileTransfer).FullName))
                {
                    var data = (DirectProfileTransfer) e.Data.GetData(typeof(DirectProfileTransfer).FullName);
                    e.Effect = data.ProcessID == Process.GetCurrentProcess().Id
                        ? data.Locked
                            ? DragDropEffects.None
                            : DragDropEffects.Move
                        : DragDropEffects.Copy;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error receiving drag/drop", ex);
            }
        }

        private void lvProfiles_DragDrop(object sender, DragEventArgs e)
        {
            // Receive drop data
            if (e.Data.GetDataPresent(typeof(DirectProfileTransfer).FullName))
            {
                var data = (DirectProfileTransfer) e.Data.GetData(typeof(DirectProfileTransfer).FullName);
                if (data.ProcessID == Process.GetCurrentProcess().Id)
                {
                    DragMoveProfile(e);
                }
                else
                {
                    if (!NoUserProfiles)
                    {
                        profileManager.Mutate(new ListMutation<ScanProfile>.Append(data.ScanProfileXml.FromXml<ScanProfile>()), selectableListView);
                    }
                }
            }
            lvProfiles.InsertionMark.Index = -1;
        }

        private void lvProfiles_DragLeave(object sender, EventArgs e)
        {
            lvProfiles.InsertionMark.Index = -1;
        }

        private void DragMoveProfile(DragEventArgs e)
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
                while (index < profileManager.Profiles.Count && profileManager.Profiles[index].IsLocked)
                {
                    index++;
                }
                profileManager.Mutate(new ListMutation<ScanProfile>.MoveTo(index), selectableListView);
            }
        }

        private void lvProfiles_DragOver(object sender, DragEventArgs e)
        {
            if (e.Effect == DragDropEffects.Move)
            {
                var index = GetDragIndex(e);
                if (index == profileManager.Profiles.Count)
                {
                    lvProfiles.InsertionMark.Index = index - 1;
                    lvProfiles.InsertionMark.AppearsAfterItem = true;
                }
                else
                {
                    lvProfiles.InsertionMark.Index = index;
                    lvProfiles.InsertionMark.AppearsAfterItem = false;
                }
            }
        }

        private int GetDragIndex(DragEventArgs e)
        {
            Point cp = lvProfiles.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = lvProfiles.GetItemAt(cp.X, cp.Y);
            if (dragToItem == null)
            {
                var items = lvProfiles.Items.Cast<ListViewItem>().ToList();
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
