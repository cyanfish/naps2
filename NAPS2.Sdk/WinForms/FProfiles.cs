using System;
using System.Collections.Generic;
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

        public FProfiles(IconButtonSizer iconButtonSizer, IScanPerformer scanPerformer, ProfileNameTracker profileNameTracker)
        {
            this.iconButtonSizer = iconButtonSizer;
            this.scanPerformer = scanPerformer;
            this.profileNameTracker = profileNameTracker;
            InitializeComponent();
        }

        public Action<ScannedImage> ImageCallback { get; set; }

        private ScanProfile SelectedProfile
        {
            get
            {
                if (lvProfiles.SelectedIndices.Count == 1)
                {
                    return ProfileManager.Current.Profiles[lvProfiles.SelectedIndices[0]];
                }
                return null;
            }
        }

        private bool SelectionLocked
        {
            get
            {
                return ProfileManager.Current.Profiles.ElementsAt(lvProfiles.SelectedIndices.OfType<int>()).Any(x => x.IsLocked);
            }
        }

        private bool NoUserProfiles => ConfigProvider.Get(c => c.NoUserProfiles) && ProfileManager.Current.Profiles.Any(x => x.IsLocked);

        protected override void OnLoad(object sender, EventArgs e)
        {
            lvProfiles.LargeImageList = ilProfileIcons.IconsList;
            btnAdd.Enabled = !NoUserProfiles;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            UpdateProfiles();
            SelectProfile(x => x.IsDefault);

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

        private void UpdateProfiles()
        {
            lvProfiles.Items.Clear();
            foreach (var profile in ProfileManager.Current.Profiles)
            {
                lvProfiles.Items.Add(profile.DisplayName,
                    profile.IsDefault
                        ? profile.IsLocked ? DEFAULT_LOCK_PROFILE_ICON_ID : DEFAULT_PROFILE_ICON_ID
                        : profile.IsLocked ? LOCK_PROFILE_ICON_ID : profile.IconID);
            }
        }

        private void SelectProfile(Func<ScanProfile, bool> pred)
        {
            int i = 0;
            foreach (var profile in ProfileManager.Current.Profiles)
            {
                if (pred(profile))
                {
                    lvProfiles.Items[i].Selected = true;
                }
                i++;
            }
            if (ProfileManager.Current.Profiles.Count == 1)
            {
                lvProfiles.Items[0].Selected = true;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var fedit = FormFactory.Create<FEditProfile>();
            fedit.ScanProfile = ConfigProvider.Get(c => c.DefaultProfileSettings);
            fedit.ShowDialog();
            if (fedit.Result)
            {
                AddProfile(fedit.ScanProfile);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lvProfiles.SelectedItems.Count > 0)
            {
                int profileIndex = lvProfiles.SelectedItems[0].Index;
                var fedit = FormFactory.Create<FEditProfile>();
                fedit.ScanProfile = ProfileManager.Current.Profiles[profileIndex];
                fedit.ShowDialog();
                if (fedit.Result)
                {
                    ProfileManager.Current.Profiles[profileIndex] = fedit.ScanProfile;
                    ProfileManager.Current.Save();
                    UpdateProfiles();
                    SelectProfile(x => x == fedit.ScanProfile);
                    lvProfiles.SelectedIndices.Add(profileIndex);
                }
                else
                {
                    // Rollback
                    ProfileManager.Current.Load();
                }
            }
        }

        private void lvProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnEdit.Enabled = lvProfiles.SelectedItems.Count == 1;
            btnDelete.Enabled = lvProfiles.SelectedItems.Count > 0 && !SelectionLocked;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lvProfiles.SelectedItems.Count > 0 && !SelectionLocked)
            {
                string message = lvProfiles.SelectedIndices.Count == 1
                    ? string.Format(MiscResources.ConfirmDeleteSingleProfile, ProfileManager.Current.Profiles[lvProfiles.SelectedIndices[0]].DisplayName)
                    : string.Format(MiscResources.ConfirmDeleteMultipleProfiles, lvProfiles.SelectedIndices.Count);
                if (MessageBox.Show(message, MiscResources.Delete, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    foreach (var profile in ProfileManager.Current.Profiles.ElementsAt(lvProfiles.SelectedIndices.OfType<int>()))
                    {
                        profileNameTracker.DeletingProfile(profile.DisplayName);
                    }
                    ProfileManager.Current.Profiles.RemoveAll(lvProfiles.SelectedIndices.OfType<int>());
                    if (ProfileManager.Current.Profiles.Count == 1)
                    {
                        ProfileManager.Current.DefaultProfile = ProfileManager.Current.Profiles.First();
                    }
                    ProfileManager.Current.Save();
                    UpdateProfiles();
                    lvProfiles_SelectedIndexChanged(null, null);
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
            if (ProfileManager.Current.Profiles.Count == 0)
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
                ProfileManager.Current.Profiles.Add(editSettingsForm.ScanProfile);
                ProfileManager.Current.DefaultProfile = editSettingsForm.ScanProfile;
                ProfileManager.Current.Save();
                UpdateProfiles();
                lvProfiles.SelectedIndices.Add(0);
            }
            if (SelectedProfile == null)
            {
                MessageBox.Show(MiscResources.SelectProfileBeforeScan, MiscResources.ChooseProfile, MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (ProfileManager.Current.DefaultProfile == null)
            {
                var profile = SelectedProfile;
                ProfileManager.Current.DefaultProfile = profile;
                UpdateProfiles();
                SelectProfile(x => x == profile);
            }
            ProfileManager.Current.Save();
            var source = scanPerformer.PerformScan(SelectedProfile, DefaultScanParams(), Handle);
            await source.ForEach(ImageCallback);
            Activate();
        }

        private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var ido = Clipboard.GetDataObject();
            var canPaste = ido != null && ido.GetDataPresent(typeof (DirectProfileTransfer).FullName);
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
                ProfileManager.Current.DefaultProfile = SelectedProfile;
                ProfileManager.Current.Save();

                UpdateProfiles();
                SelectProfile(x => x.IsDefault);
            }
        }

        private void AddProfile(ScanProfile profile)
        {
            ProfileManager.Current.Profiles.Add(profile);
            if (ProfileManager.Current.Profiles.Count == 1)
            {
                ProfileManager.Current.DefaultProfile = profile;
            }
            UpdateProfiles();
            SelectProfile(x => x == profile);
            ProfileManager.Current.Save();
        }

        private IDataObject GetSelectedProfileDataObject()
        {
            IDataObject ido = new DataObject();
            int profileIndex = lvProfiles.SelectedItems[0].Index;
            var profile = ProfileManager.Current.Profiles[profileIndex];
            ido.SetData(typeof(DirectProfileTransfer), new DirectProfileTransfer(profile));
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
                var data = (DirectProfileTransfer)ido.GetData(typeof(DirectProfileTransfer).FullName);
                AddProfile(data.ScanProfile);
            }
        }

        #region Drag/Drop

        private void lvProfiles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Provide drag data
            if (lvProfiles.SelectedItems.Count > 0)
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
                    var data = (DirectProfileTransfer)e.Data.GetData(typeof(DirectProfileTransfer).FullName);
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
                var data = (DirectProfileTransfer)e.Data.GetData(typeof(DirectProfileTransfer).FullName);
                if (data.ProcessID == Process.GetCurrentProcess().Id)
                {
                    DragMoveProfile(e);
                }
                else
                {
                    if (!NoUserProfiles)
                    {
                        AddProfile(data.ScanProfile);
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
            if (lvProfiles.SelectedItems.Count == 0)
            {
                return;
            }
            int index = GetDragIndex(e);
            if (index != -1)
            {
                var selectedIndex = lvProfiles.SelectedItems[0].Index;
                var selectedProfile = ProfileManager.Current.Profiles[selectedIndex];
                if (selectedProfile.IsLocked)
                {
                    return;
                }
                while (index < ProfileManager.Current.Profiles.Count && ProfileManager.Current.Profiles[index].IsLocked)
                {
                    index++;
                }
                ProfileManager.Current.Profiles.RemoveAt(selectedIndex);
                if (index > selectedIndex)
                {
                    index--;
                }
                ProfileManager.Current.Profiles.Insert(index, selectedProfile);
                UpdateProfiles();
                SelectProfile(x => x == selectedProfile);
                ProfileManager.Current.Save();
            }
        }

        private void lvProfiles_DragOver(object sender, DragEventArgs e)
        {
            if (e.Effect == DragDropEffects.Move)
            {
                var index = GetDragIndex(e);
                if (index == ProfileManager.Current.Profiles.Count)
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
