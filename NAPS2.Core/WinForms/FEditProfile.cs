using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;
using NAPS2.Util;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FEditProfile : FormBase
    {
        private readonly IScanDriverFactory driverFactory;
        private readonly IErrorOutput errorOutput;
        private readonly ProfileNameTracker profileNameTracker;
        private readonly AppConfigManager appConfigManager;

        private ScanProfile scanProfile;
        private ScanDevice currentDevice;
        private bool isDefault;

        private int iconID;
        private bool result;

        private bool suppressChangeEvent;

        public FEditProfile(IScanDriverFactory driverFactory, IErrorOutput errorOutput, ProfileNameTracker profileNameTracker, AppConfigManager appConfigManager)
        {
            this.driverFactory = driverFactory;
            this.errorOutput = errorOutput;
            this.profileNameTracker = profileNameTracker;
            this.appConfigManager = appConfigManager;
            InitializeComponent();
            AddEnumItems<ScanHorizontalAlign>(cmbAlign);
            AddEnumItems<ScanBitDepth>(cmbDepth);
            AddEnumItems<ScanDpi>(cmbResolution);
            AddEnumItems<ScanScale>(cmbScale);
            AddEnumItems<ScanSource>(cmbSource);
            CmbPage.Format += (sender, e) =>
            {
                var item = (PageSizeListItem)e.ListItem;
                e.Value = item.Label;
            };
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            // Don't trigger any onChange events
            suppressChangeEvent = true;

            pctIcon.Image = ilProfileIcons.IconsList.Images[ScanProfile.IconID];
            txtName.Text = ScanProfile.DisplayName;
            if (CurrentDevice == null)
            {
                CurrentDevice = ScanProfile.Device;
            }
            isDefault = ScanProfile.IsDefault;
            iconID = ScanProfile.IconID;

            cmbSource.SelectedIndex = (int)ScanProfile.PaperSource;
            cmbDepth.SelectedIndex = (int)ScanProfile.BitDepth;
            cmbResolution.SelectedIndex = (int)ScanProfile.Resolution;
            TxtContrast.Text = ScanProfile.Contrast.ToString("G");
            TxtBrightness.Text = ScanProfile.Brightness.ToString("G");
            UpdatePageSizeList();
            SelectPageSize();
            cmbScale.SelectedIndex = (int)ScanProfile.AfterScanScale;
            cmbAlign.SelectedIndex = (int)ScanProfile.PageAlign;

            CbAutoSave.Checked = ScanProfile.EnableAutoSave;

            // The setter updates the driver selection checkboxes
            DeviceDriverName = ScanProfile.DriverName;

            rdbNative.Checked = ScanProfile.UseNativeUI;
            RdbConfig.Checked = !ScanProfile.UseNativeUI;

            // Start triggering onChange events again
            suppressChangeEvent = false;

            UpdateEnabledControls();

            LinkAutoSaveSettings.Location = new Point(CbAutoSave.Right, LinkAutoSaveSettings.Location.Y);
            new LayoutManager(this)
                .Bind(txtName, TxtDevice, panel1, panel2)
                    .WidthToForm()
                .Bind(pctIcon, BtnChooseDevice, BtnOK, BtnCancel)
                    .RightToForm()
                .Bind(cmbAlign, cmbDepth, CmbPage, cmbResolution, cmbScale, cmbSource, TrBrightness, TrContrast, RdbConfig, rdbNative)
                    .WidthTo(() => Width / 2)
                .Bind(rdTWAIN, rdbNative, Label3, cmbDepth, Label9, cmbAlign, Label10, cmbScale, Label7, TrContrast)
                    .LeftTo(() => Width / 2)
                .Bind(TxtBrightness)
                    .LeftTo(() => TrBrightness.Right)
                .Bind(TxtContrast)
                    .LeftTo(() => TrContrast.Right)
                .Activate();
        }

        private void UpdatePageSizeList()
        {
            CmbPage.Items.Clear();

            // Defaults
            foreach (ScanPageSize item in Enum.GetValues(typeof(ScanPageSize)))
            {
                CmbPage.Items.Add(new PageSizeListItem
                {
                    Type = item,
                    Label = item.Description()
                });
            }

            // Custom Presets
            foreach (var preset in UserConfigManager.Config.CustomPageSizePresets.OrderBy(x => x.Name))
            {
                CmbPage.Items.Insert(CmbPage.Items.Count - 1, new PageSizeListItem
                {
                    Type = ScanPageSize.Custom,
                    Label = string.Format(MiscResources.NamedPageSizeFormat, preset.Name, preset.Dimens.Width, preset.Dimens.Height, preset.Dimens.Unit.Description()),
                    CustomName = preset.Name,
                    CustomDimens = preset.Dimens
                });
            }
        }

        private void SelectPageSize()
        {
            if (ScanProfile.PageSize == ScanPageSize.Custom)
            {
                SelectCustomPageSize(ScanProfile.CustomPageSizeName, ScanProfile.CustomPageSize);
            }
            else
            {
                CmbPage.SelectedIndex = (int)ScanProfile.PageSize;
            }
        }

        private void SelectCustomPageSize(string name, PageDimensions dimens)
        {
            for (int i = 0; i < CmbPage.Items.Count; i++)
            {
                var item = (PageSizeListItem)CmbPage.Items[i];
                if (item.Type == ScanPageSize.Custom && item.CustomName == name && item.CustomDimens == dimens)
                {
                    CmbPage.SelectedIndex = i;
                    return;
                }
            }

            // Not found, so insert a new item
            CmbPage.Items.Insert(CmbPage.Items.Count - 1, new PageSizeListItem
            {
                Type = ScanPageSize.Custom,
                Label = string.IsNullOrEmpty(name)
                    ? string.Format(MiscResources.CustomPageSizeFormat, dimens.Width, dimens.Height, dimens.Unit.Description())
                    : string.Format(MiscResources.NamedPageSizeFormat, name, dimens.Width, dimens.Height, dimens.Unit.Description()),
                CustomName = name,
                CustomDimens = dimens
            });
            CmbPage.SelectedIndex = CmbPage.Items.Count - 2;
        }

        public bool Result => result;

        public ScanProfile ScanProfile
        {
            get => scanProfile;
            set => scanProfile = value.Clone();
        }

        private string DeviceDriverName
        {
            get => rdTWAIN.Checked ? TwainScanDriver.DRIVER_NAME : WiaScanDriver.DRIVER_NAME;
            set
            {
                if (value == TwainScanDriver.DRIVER_NAME)
                {
                    rdTWAIN.Checked = true;
                }
                else
                {
                    RdWIA.Checked = true;
                }
            }
        }

        public ScanDevice CurrentDevice
        {
            get => currentDevice;
            set
            {
                currentDevice = value;
                TxtDevice.Text = (value == null ? "" : value.Name);
            }
        }

        private void ChooseDevice(string driverName)
        {
            var driver = driverFactory.Create(driverName);
            try
            {
                driver.DialogParent = this;
                driver.ScanProfile = ScanProfile;
                ScanDevice device = driver.PromptForDevice();
                if (device != null)
                {
                    if (string.IsNullOrEmpty(txtName.Text)
                        || (CurrentDevice != null && CurrentDevice.Name == txtName.Text))
                    {
                        txtName.Text = device.Name;
                    }
                    CurrentDevice = device;
                }
            }
            catch (ScanDriverException e)
            {
                if (e is ScanDriverUnknownException)
                {
                    Log.ErrorException(e.Message, e.InnerException);
                    errorOutput.DisplayError(e.Message, e);
                }
                else
                {
                    errorOutput.DisplayError(e.Message);
                }
            }
        }

        private void BtnChooseDevice_Click(object sender, EventArgs e)
        {
            ChooseDevice(DeviceDriverName);
        }

        private void SaveSettings()
        {
            if (ScanProfile.IsLocked)
            {
                if (!ScanProfile.IsDeviceLocked)
                {
                    ScanProfile.Device = CurrentDevice;
                }
                return;
            }
            var pageSize = (PageSizeListItem)CmbPage.SelectedItem;
            if (ScanProfile.DisplayName != null)
            {
                profileNameTracker.RenamingProfile(ScanProfile.DisplayName, txtName.Text);
            }
            scanProfile = new ScanProfile
            {
                Version = ScanProfile.CURRENT_VERSION,

                Device = CurrentDevice,
                IsDefault = isDefault,
                DriverName = DeviceDriverName,
                DisplayName = txtName.Text,
                IconID = iconID,
                MaxQuality = ScanProfile.MaxQuality,
                UseNativeUI = rdbNative.Checked,

                AfterScanScale = (ScanScale)cmbScale.SelectedIndex,
                BitDepth = (ScanBitDepth)cmbDepth.SelectedIndex,
                Brightness = TrBrightness.Value,
                Contrast = TrContrast.Value,
                PageAlign = (ScanHorizontalAlign)cmbAlign.SelectedIndex,
                PageSize = pageSize.Type,
                CustomPageSizeName = pageSize.CustomName,
                CustomPageSize = pageSize.CustomDimens,
                Resolution = (ScanDpi)cmbResolution.SelectedIndex,
                PaperSource = (ScanSource)cmbSource.SelectedIndex,

                EnableAutoSave = CbAutoSave.Checked,
                AutoSaveSettings = ScanProfile.AutoSaveSettings,
                Quality = ScanProfile.Quality,
                BrightnessContrastAfterScan = ScanProfile.BrightnessContrastAfterScan,
                AutoDeskew = ScanProfile.AutoDeskew,
                WiaOffsetWidth = ScanProfile.WiaOffsetWidth,
                WiaRetryOnFailure = ScanProfile.WiaRetryOnFailure,
                WiaDelayBetweenScans = ScanProfile.WiaDelayBetweenScans,
                WiaDelayBetweenScansSeconds = ScanProfile.WiaDelayBetweenScansSeconds,
                ForcePageSize = ScanProfile.ForcePageSize,
                ForcePageSizeCrop = ScanProfile.ForcePageSizeCrop,
                FlipDuplexedPages = ScanProfile.FlipDuplexedPages,
                TwainImpl = ScanProfile.TwainImpl,

                ExcludeBlankPages = ScanProfile.ExcludeBlankPages,
                BlankPageWhiteThreshold = ScanProfile.BlankPageWhiteThreshold,
                BlankPageCoverageThreshold = ScanProfile.BlankPageCoverageThreshold
            };
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // Note: If CurrentDevice is null, that's fine. A prompt will be shown when scanning.

            if (txtName.Text?.Length == 0)
            {
                errorOutput.DisplayError(MiscResources.NameMissing);
                return;
            }
            result = true;
            SaveSettings();
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void RdbConfig_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledControls();
        }

        private void RdbNativeWIA_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledControls();
        }

        private void UpdateEnabledControls()
        {
            if (!suppressChangeEvent)
            {
                suppressChangeEvent = true;

                bool locked = ScanProfile.IsLocked;
                bool deviceLocked = ScanProfile.IsDeviceLocked;
                bool settingsEnabled = !locked && RdbConfig.Checked;

                txtName.Enabled = !locked;
                RdWIA.Enabled = rdTWAIN.Enabled = !locked;
                TxtDevice.Enabled = !deviceLocked;
                BtnChooseDevice.Enabled = !deviceLocked;
                RdbConfig.Enabled = rdbNative.Enabled = !locked;

                cmbSource.Enabled = settingsEnabled;
                cmbResolution.Enabled = settingsEnabled;
                CmbPage.Enabled = settingsEnabled;
                cmbDepth.Enabled = settingsEnabled;
                cmbAlign.Enabled = settingsEnabled;
                cmbScale.Enabled = settingsEnabled;
                TrBrightness.Enabled = settingsEnabled;
                TrContrast.Enabled = settingsEnabled;
                TxtBrightness.Enabled = settingsEnabled;
                TxtContrast.Enabled = settingsEnabled;

                CbAutoSave.Enabled = !locked && !appConfigManager.Config.DisableAutoSave;
                LinkAutoSaveSettings.Visible = !locked && !appConfigManager.Config.DisableAutoSave;

                BtnAdvanced.Enabled = !locked;

                suppressChangeEvent = false;
            }
        }

        private void RdWIA_CheckedChanged(object sender, EventArgs e)
        {
            if (!suppressChangeEvent)
            {
                ScanProfile.Device = null;
                CurrentDevice = null;
                UpdateEnabledControls();
            }
        }

        private void TxtBrightness_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(TxtBrightness.Text, out int value))
            {
                if (value >= TrBrightness.Minimum && value <= TrBrightness.Maximum)
                {
                    TrBrightness.Value = value;
                }
            }
        }

        private void TrBrightness_Scroll(object sender, EventArgs e)
        {
            TxtBrightness.Text = TrBrightness.Value.ToString("G");
        }

        private void TxtContrast_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(TxtContrast.Text, out int value))
            {
                if (value >= TrContrast.Minimum && value <= TrContrast.Maximum)
                {
                    TrContrast.Value = value;
                }
            }
        }

        private void TrContrast_Scroll(object sender, EventArgs e)
        {
            TxtContrast.Text = TrContrast.Value.ToString("G");
        }

        private int lastPageSizeIndex = -1;
        private PageSizeListItem lastPageSizeItem;

        private void CmbPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CmbPage.SelectedIndex == CmbPage.Items.Count - 1)
            {
                // "Custom..." selected
                var form = FormFactory.Create<FPageSize>();
                form.PageSizeDimens = lastPageSizeItem.Type == ScanPageSize.Custom
                    ? lastPageSizeItem.CustomDimens
                    : lastPageSizeItem.Type.PageDimensions();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    UpdatePageSizeList();
                    SelectCustomPageSize(form.PageSizeName, form.PageSizeDimens);
                }
                else
                {
                    CmbPage.SelectedIndex = lastPageSizeIndex;
                }
            }
            lastPageSizeIndex = CmbPage.SelectedIndex;
            lastPageSizeItem = (PageSizeListItem)CmbPage.SelectedItem;
        }

        private void LinkAutoSaveSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (appConfigManager.Config.DisableAutoSave)
            {
                return;
            }
            var form = FormFactory.Create<FAutoSaveSettings>();
            ScanProfile.DriverName = DeviceDriverName;
            form.ScanProfile = ScanProfile;
            form.ShowDialog();
        }

        private void BtnAdvanced_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FAdvancedScanSettings>();
            ScanProfile.DriverName = DeviceDriverName;
            ScanProfile.BitDepth = (ScanBitDepth)cmbDepth.SelectedIndex;
            form.ScanProfile = ScanProfile;
            form.ShowDialog();
        }

        private void CbAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            if (!suppressChangeEvent)
            {
                if (CbAutoSave.Checked)
                {
                    LinkAutoSaveSettings.Enabled = true;
                    var form = FormFactory.Create<FAutoSaveSettings>();
                    form.ScanProfile = ScanProfile;
                    form.ShowDialog();
                    CbAutoSave.Checked &= form.Result;
                }
            }
            LinkAutoSaveSettings.Enabled = CbAutoSave.Checked;
        }

        private void TxtDevice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                CurrentDevice = null;
            }
        }

        private class PageSizeListItem
        {
            public string Label { get; set; }

            public ScanPageSize Type { get; set; }

            public string CustomName { get; set; }

            public PageDimensions CustomDimens { get; set; }
        }
    }
}