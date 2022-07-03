using System.Drawing;
using System.Windows.Forms;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;

namespace NAPS2.WinForms;

public partial class FEditProfile : FormBase
{
    private readonly IScanPerformer _scanPerformer;
    private readonly ErrorOutput _errorOutput;
    private readonly ProfileNameTracker _profileNameTracker;

    private ScanProfile _scanProfile;
    private ScanDevice _currentDevice;
    private bool _isDefault;
    private bool _useProxy;

    private int _iconId;
    private bool _result;

    private bool _suppressChangeEvent;

    public FEditProfile(IScanPerformer scanPerformer, ErrorOutput errorOutput, ProfileNameTracker profileNameTracker)
    {
        _scanPerformer = scanPerformer;
        _errorOutput = errorOutput;
        _profileNameTracker = profileNameTracker;
        InitializeComponent();
        btnNetwork.Left = btnChooseDevice.Right + 6;
        // TODO: Remove this to reenable
        btnNetwork.Visible = false;

        AddEnumItems<ScanHorizontalAlign>(cmbAlign);
        AddEnumItems<ScanBitDepth>(cmbDepth);
        AddEnumItems<ScanDpi>(cmbResolution);
        AddEnumItems<ScanScale>(cmbScale);
        AddEnumItems<ScanSource>(cmbSource);
        cmbPage.Format += (sender, e) =>
        {
            var item = (PageSizeListItem)e.ListItem;
            e.Value = item.Label;
        };

        rdWIA.Visible = PlatformCompat.System.IsWiaDriverSupported;
        rdTWAIN.Visible = PlatformCompat.System.IsTwainDriverSupported;
        rdSANE.Visible = PlatformCompat.System.IsSaneDriverSupported;
    }

    protected override void OnLoad(object sender, EventArgs e)
    {
        // Don't trigger any onChange events
        _suppressChangeEvent = true;

        pctIcon.Image = ilProfileIcons.IconsList.Images[ScanProfile.IconID];
        txtName.Text = ScanProfile.DisplayName;
        if (CurrentDevice == null)
        {
            CurrentDevice = ScanProfile.Device;
        }
        _isDefault = ScanProfile.IsDefault;
        _useProxy = ScanProfile.DriverName == DriverNames.PROXY;
        _iconId = ScanProfile.IconID;

        cmbSource.SelectedIndex = (int)ScanProfile.PaperSource;
        cmbDepth.SelectedIndex = (int)ScanProfile.BitDepth;
        cmbResolution.SelectedIndex = (int)ScanProfile.Resolution;
        txtContrast.Text = ScanProfile.Contrast.ToString("G");
        txtBrightness.Text = ScanProfile.Brightness.ToString("G");
        UpdatePageSizeList();
        SelectPageSize();
        cmbScale.SelectedIndex = (int)ScanProfile.AfterScanScale;
        cmbAlign.SelectedIndex = (int)ScanProfile.PageAlign;

        cbAutoSave.Checked = ScanProfile.EnableAutoSave;

        // The setter updates the driver selection checkboxes
        DeviceDriverName = _useProxy ? ScanProfile.ProxyDriverName : ScanProfile.DriverName;

        rdbNative.Checked = ScanProfile.UseNativeUI;
        rdbConfig.Checked = !ScanProfile.UseNativeUI;

        // Start triggering onChange events again
        _suppressChangeEvent = false;

        UpdateEnabledControls();

        linkAutoSaveSettings.Location = new Point(cbAutoSave.Right, linkAutoSaveSettings.Location.Y);
        new LayoutManager(this)
            .Bind(txtName, txtDevice, panelUI, panel2)
            .WidthToForm()
            .Bind(pctIcon, btnChooseDevice, btnNetwork, btnOK, btnCancel)
            .RightToForm()
            .Bind(cmbAlign, cmbDepth, cmbPage, cmbResolution, cmbScale, cmbSource, trBrightness, trContrast, rdbConfig, rdbNative)
            .WidthTo(() => Width / 2)
            .Bind(rdTWAIN, rdbNative, label3, cmbDepth, label9, cmbAlign, label10, cmbScale, label7, trContrast)
            .LeftTo(() => Width / 2)
            .Bind(txtBrightness)
            .LeftTo(() => trBrightness.Right)
            .Bind(txtContrast)
            .LeftTo(() => trContrast.Right)
            .Activate();
    }

    private void UpdatePageSizeList()
    {
        cmbPage.Items.Clear();

        // Defaults
        foreach (ScanPageSize item in Enum.GetValues(typeof(ScanPageSize)))
        {
            cmbPage.Items.Add(new PageSizeListItem
            {
                Type = item,
                Label = item.Description()
            });
        }

        // Custom Presets
        foreach (var preset in Config.Get(c => c.CustomPageSizePresets).OrderBy(x => x.Name))
        {
            cmbPage.Items.Insert(cmbPage.Items.Count - 1, new PageSizeListItem
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
            if (ScanProfile.CustomPageSizeName != null && ScanProfile.CustomPageSize != null)
            {
                SelectCustomPageSize(ScanProfile.CustomPageSizeName, ScanProfile.CustomPageSize);
            }
            else
            {
                cmbPage.SelectedIndex = 0;
            }
        }
        else
        {
            cmbPage.SelectedIndex = (int) ScanProfile.PageSize;
        }
    }

    private void SelectCustomPageSize(string name, PageDimensions dimens)
    {
        for (int i = 0; i < cmbPage.Items.Count; i++)
        {
            var item = (PageSizeListItem) cmbPage.Items[i];
            if (item.Type == ScanPageSize.Custom && item.CustomName == name && item.CustomDimens == dimens)
            {
                cmbPage.SelectedIndex = i;
                return;
            }
        }

        // Not found, so insert a new item
        cmbPage.Items.Insert(cmbPage.Items.Count - 1, new PageSizeListItem
        {
            Type = ScanPageSize.Custom,
            Label = string.IsNullOrEmpty(name)
                ? string.Format(MiscResources.CustomPageSizeFormat, dimens.Width, dimens.Height, dimens.Unit.Description())
                : string.Format(MiscResources.NamedPageSizeFormat, name, dimens.Width, dimens.Height, dimens.Unit.Description()),
            CustomName = name,
            CustomDimens = dimens
        });
        cmbPage.SelectedIndex = cmbPage.Items.Count - 2;
    }

    public bool Result => _result;

    public ScanProfile ScanProfile
    {
        get => _scanProfile;
        set => _scanProfile = value.Clone();
    }

    private string DeviceDriverName
    {
        get => rdTWAIN.Checked ? DriverNames.TWAIN
            : rdSANE.Checked  ? DriverNames.SANE
            : DriverNames.WIA;
        set
        {
            if (value == DriverNames.TWAIN)
            {
                rdTWAIN.Checked = true;
            }
            else if (value == DriverNames.SANE)
            {
                rdSANE.Checked = true;
            }
            else if (value == DriverNames.WIA || PlatformCompat.System.IsWiaDriverSupported)
            {
                rdWIA.Checked = true;
            }
            else
            {
                rdSANE.Checked = true;
            }
        }
    }

    public ScanDevice CurrentDevice
    {
        get => _currentDevice;
        set
        {
            _currentDevice = value;
            txtDevice.Text = (value == null ? "" : value.Name);
        }
    }

    private async Task ChooseDevice()
    {
        try
        {
            ScanDevice device = await _scanPerformer.PromptForDevice(ScanProfile, Handle);
            if (device != null)
            {
                if (string.IsNullOrEmpty(txtName.Text) ||
                    CurrentDevice != null && CurrentDevice.Name == txtName.Text)
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
                _errorOutput.DisplayError(e.Message, e);
            }
            else
            {
                _errorOutput.DisplayError(e.Message);
            }
        }
    }

    private async void btnChooseDevice_Click(object sender, EventArgs e)
    {
        ScanProfile.DriverName = _useProxy ? DriverNames.PROXY : DeviceDriverName;
        ScanProfile.ProxyDriverName = _useProxy ? DeviceDriverName : null;
        await ChooseDevice();
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
        var pageSize = (PageSizeListItem) cmbPage.SelectedItem;
        if (ScanProfile.DisplayName != null)
        {
            _profileNameTracker.RenamingProfile(ScanProfile.DisplayName, txtName.Text);
        }
        _scanProfile = new ScanProfile
        {
            Version = ScanProfile.CURRENT_VERSION,

            Device = CurrentDevice,
            IsDefault = _isDefault,
            DriverName = _useProxy ? DriverNames.PROXY : DeviceDriverName,
            ProxyConfig = ScanProfile.ProxyConfig,
            ProxyDriverName = _useProxy ? DeviceDriverName : null,
            DisplayName = txtName.Text,
            IconID = _iconId,
            MaxQuality = ScanProfile.MaxQuality,
            UseNativeUI = rdbNative.Checked,

            AfterScanScale = (ScanScale)cmbScale.SelectedIndex,
            BitDepth = (ScanBitDepth)cmbDepth.SelectedIndex,
            Brightness = trBrightness.Value,
            Contrast = trContrast.Value,
            PageAlign = (ScanHorizontalAlign)cmbAlign.SelectedIndex,
            PageSize = pageSize.Type,
            CustomPageSizeName = pageSize.CustomName,
            CustomPageSize = pageSize.CustomDimens,
            Resolution = (ScanDpi)cmbResolution.SelectedIndex,
            PaperSource = (ScanSource)cmbSource.SelectedIndex,

            EnableAutoSave = cbAutoSave.Checked,
            AutoSaveSettings = ScanProfile.AutoSaveSettings,
            Quality = ScanProfile.Quality,
            BrightnessContrastAfterScan = ScanProfile.BrightnessContrastAfterScan,
            AutoDeskew = ScanProfile.AutoDeskew,
            WiaOffsetWidth = ScanProfile.WiaOffsetWidth,
            WiaRetryOnFailure = ScanProfile.WiaRetryOnFailure,
            WiaDelayBetweenScans = ScanProfile.WiaDelayBetweenScans,
            WiaDelayBetweenScansSeconds = ScanProfile.WiaDelayBetweenScansSeconds,
            WiaVersion = ScanProfile.WiaVersion,
            ForcePageSize = ScanProfile.ForcePageSize,
            ForcePageSizeCrop = ScanProfile.ForcePageSizeCrop,
            FlipDuplexedPages = ScanProfile.FlipDuplexedPages,
            TwainImpl = ScanProfile.TwainImpl,

            ExcludeBlankPages = ScanProfile.ExcludeBlankPages,
            BlankPageWhiteThreshold = ScanProfile.BlankPageWhiteThreshold,
            BlankPageCoverageThreshold = ScanProfile.BlankPageCoverageThreshold
        };
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        // Note: If CurrentDevice is null, that's fine. A prompt will be shown when scanning.

        if (txtName.Text == "")
        {
            _errorOutput.DisplayError(MiscResources.NameMissing);
            return;
        }
        _result = true;
        SaveSettings();
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void rdbConfig_CheckedChanged(object sender, EventArgs e)
    {
        UpdateEnabledControls();
    }

    private void rdbNativeWIA_CheckedChanged(object sender, EventArgs e)
    {
        UpdateEnabledControls();
    }

    private void UpdateEnabledControls()
    {
        if (!_suppressChangeEvent)
        {
            _suppressChangeEvent = true;

            bool canUseNativeUi = DeviceDriverName != DriverNames.SANE && !_useProxy;
            bool locked = ScanProfile.IsLocked;
            bool deviceLocked = ScanProfile.IsDeviceLocked;
            bool settingsEnabled = !locked && (rdbConfig.Checked || !canUseNativeUi);

            txtName.Enabled = !locked;
            rdWIA.Enabled = rdTWAIN.Enabled = rdSANE.Enabled = !locked;
            txtDevice.Enabled = !deviceLocked;
            btnChooseDevice.Enabled = !deviceLocked;
            rdbConfig.Enabled = rdbNative.Enabled = !locked;

            cmbSource.Enabled = settingsEnabled;
            cmbResolution.Enabled = settingsEnabled;
            cmbPage.Enabled = settingsEnabled;
            cmbDepth.Enabled = settingsEnabled;
            cmbAlign.Enabled = settingsEnabled;
            cmbScale.Enabled = settingsEnabled;
            trBrightness.Enabled = settingsEnabled;
            trContrast.Enabled = settingsEnabled;
            txtBrightness.Enabled = settingsEnabled;
            txtContrast.Enabled = settingsEnabled;

            cbAutoSave.Enabled = !locked && !Config.Get(c => c.DisableAutoSave);
            linkAutoSaveSettings.Visible = !locked && !Config.Get(c => c.DisableAutoSave);

            btnAdvanced.Enabled = !locked;

            ConditionalControls.UnlockHeight(this);
            ConditionalControls.SetVisible(panelUI, canUseNativeUi, 20);
            ConditionalControls.LockHeight(this);

            _suppressChangeEvent = false;
        }
    }

    private void rdDriver_CheckedChanged(object sender, EventArgs e)
    {
        if (((RadioButton)sender).Checked && !_suppressChangeEvent)
        {
            ScanProfile.Device = null;
            CurrentDevice = null;
            UpdateEnabledControls();
        }
    }

    private void txtBrightness_TextChanged(object sender, EventArgs e)
    {
        if (int.TryParse(txtBrightness.Text, out int value))
        {
            if (value >= trBrightness.Minimum && value <= trBrightness.Maximum)
            {
                trBrightness.Value = value;
            }
        }
    }

    private void trBrightness_Scroll(object sender, EventArgs e)
    {
        txtBrightness.Text = trBrightness.Value.ToString("G");
    }

    private void txtContrast_TextChanged(object sender, EventArgs e)
    {
        if (int.TryParse(txtContrast.Text, out int value))
        {
            if (value >= trContrast.Minimum && value <= trContrast.Maximum)
            {
                trContrast.Value = value;
            }
        }
    }

    private void trContrast_Scroll(object sender, EventArgs e)
    {
        txtContrast.Text = trContrast.Value.ToString("G");
    }

    private int _lastPageSizeIndex = -1;
    private PageSizeListItem _lastPageSizeItem = null;

    private void cmbPage_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbPage.SelectedIndex == cmbPage.Items.Count - 1)
        {
            // "Custom..." selected
            var form = FormFactory.Create<FPageSize>();
            form.PageSizeDimens = _lastPageSizeItem.Type == ScanPageSize.Custom
                ? _lastPageSizeItem.CustomDimens
                : _lastPageSizeItem.Type.PageDimensions();
            if (form.ShowDialog() == DialogResult.OK)
            {
                UpdatePageSizeList();
                SelectCustomPageSize(form.PageSizeName, form.PageSizeDimens);
            }
            else
            {
                cmbPage.SelectedIndex = _lastPageSizeIndex;
            }
        }
        _lastPageSizeIndex = cmbPage.SelectedIndex;
        _lastPageSizeItem = (PageSizeListItem)cmbPage.SelectedItem;
    }

    private void linkAutoSaveSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        if (Config.Get(c => c.DisableAutoSave))
        {
            return;
        }
        var form = FormFactory.Create<FAutoSaveSettings>();
        ScanProfile.DriverName = DeviceDriverName;
        form.ScanProfile = ScanProfile;
        form.ShowDialog();
    }

    private void btnAdvanced_Click(object sender, EventArgs e)
    {
        var form = FormFactory.Create<FAdvancedScanSettings>();
        ScanProfile.DriverName = DeviceDriverName;
        ScanProfile.BitDepth = (ScanBitDepth)cmbDepth.SelectedIndex;
        form.ScanProfile = ScanProfile;
        form.ShowDialog();
    }

    private void cbAutoSave_CheckedChanged(object sender, EventArgs e)
    {
        if (!_suppressChangeEvent)
        {
            if (cbAutoSave.Checked)
            {
                linkAutoSaveSettings.Enabled = true;
                var form = FormFactory.Create<FAutoSaveSettings>();
                form.ScanProfile = ScanProfile;
                form.ShowDialog();
                if (!form.Result)
                {
                    cbAutoSave.Checked = false;
                }
            }
        }
        linkAutoSaveSettings.Enabled = cbAutoSave.Checked;
    }

    private void txtDevice_KeyDown(object sender, KeyEventArgs e)
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