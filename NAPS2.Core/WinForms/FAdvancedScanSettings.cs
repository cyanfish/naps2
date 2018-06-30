using NAPS2.Config;
using NAPS2.Scan;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;
using System;

namespace NAPS2.WinForms
{
    public partial class FAdvancedScanSettings : FormBase
    {
        private readonly AppConfigManager appConfigManager;

        public FAdvancedScanSettings(AppConfigManager appConfigManager)
        {
            this.appConfigManager = appConfigManager;
            InitializeComponent();

            cmbTwainImpl.Format += (sender, e) => e.Value = ((Enum)e.ListItem).Description();
            cmbTwainImpl.Items.Add(TwainImpl.Default);
            cmbTwainImpl.Items.Add(TwainImpl.MemXfer);
            cmbTwainImpl.Items.Add(TwainImpl.OldDsm);
            cmbTwainImpl.Items.Add(TwainImpl.Legacy);
            if (Environment.Is64BitProcess)
            {
                cmbTwainImpl.Items.Add(TwainImpl.X64);
            }
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            UpdateValues(ScanProfile);
            UpdateEnabled();

            new LayoutManager(this)
                .Bind(GroupBox1, GroupBox2, GroupBox3, tbImageQuality, TbWhiteThreshold, TbCoverageThreshold)
                    .WidthToForm()
                .Bind(txtImageQuality, TxtWhiteThreshold, TxtCoverageThreshold, BtnOK, BtnCancel)
                    .RightToForm()
                .Activate();
        }

        private void UpdateValues(ScanProfile scanProfile)
        {
            CbHighQuality.Checked = scanProfile.MaxQuality;
            tbImageQuality.Value = scanProfile.Quality;
            txtImageQuality.Text = scanProfile.Quality.ToString("G");
            cbBrightnessContrastAfterScan.Checked = scanProfile.BrightnessContrastAfterScan;
            CbAutoDeskew.Checked = scanProfile.AutoDeskew;
            CbWiaOffsetWidth.Checked = scanProfile.WiaOffsetWidth;
            CbWiaRetryOnFailure.Checked = scanProfile.WiaRetryOnFailure;
            CbWiaDelayBetweenScans.Checked = scanProfile.WiaDelayBetweenScans;
            txtWiaDelayBetweenScansSeconds.Text = scanProfile.WiaDelayBetweenScansSeconds.ToString("G");
            CbForcePageSize.Checked = scanProfile.ForcePageSize;
            CbForcePageSizeCrop.Checked = scanProfile.ForcePageSizeCrop;
            cbFlipDuplex.Checked = scanProfile.FlipDuplexedPages;
            if (scanProfile.TwainImpl != TwainImpl.X64 || Environment.Is64BitProcess)
            {
                cmbTwainImpl.SelectedIndex = (int)scanProfile.TwainImpl;
            }
            CbExcludeBlankPages.Checked = scanProfile.ExcludeBlankPages;
            TbWhiteThreshold.Value = scanProfile.BlankPageWhiteThreshold;
            TxtWhiteThreshold.Text = scanProfile.BlankPageWhiteThreshold.ToString("G");
            TbCoverageThreshold.Value = scanProfile.BlankPageCoverageThreshold;
            TxtCoverageThreshold.Text = scanProfile.BlankPageCoverageThreshold.ToString("G");
        }

        private void UpdateEnabled()
        {
            cmbTwainImpl.Enabled = ScanProfile.DriverName == TwainScanDriver.DRIVER_NAME;
            CbWiaOffsetWidth.Enabled = ScanProfile.DriverName == WiaScanDriver.DRIVER_NAME;
            CbWiaRetryOnFailure.Enabled = ScanProfile.DriverName == WiaScanDriver.DRIVER_NAME;
            CbWiaDelayBetweenScans.Enabled = ScanProfile.DriverName == WiaScanDriver.DRIVER_NAME;
            txtWiaDelayBetweenScansSeconds.Enabled = ScanProfile.DriverName == WiaScanDriver.DRIVER_NAME && CbWiaDelayBetweenScans.Checked;
            tbImageQuality.Enabled = !CbHighQuality.Checked;
            txtImageQuality.Enabled = !CbHighQuality.Checked;
            TbWhiteThreshold.Enabled = CbExcludeBlankPages.Checked && ScanProfile.BitDepth != ScanBitDepth.BlackWhite;
            TxtWhiteThreshold.Enabled = CbExcludeBlankPages.Checked && ScanProfile.BitDepth != ScanBitDepth.BlackWhite;
            TbCoverageThreshold.Enabled = CbExcludeBlankPages.Checked;
            TxtCoverageThreshold.Enabled = CbExcludeBlankPages.Checked;
        }

        public ScanProfile ScanProfile { get; set; }

        private void SaveSettings()
        {
            ScanProfile.Quality = tbImageQuality.Value;
            ScanProfile.MaxQuality = CbHighQuality.Checked;
            ScanProfile.BrightnessContrastAfterScan = cbBrightnessContrastAfterScan.Checked;
            ScanProfile.AutoDeskew = CbAutoDeskew.Checked;
            ScanProfile.WiaOffsetWidth = CbWiaOffsetWidth.Checked;
            ScanProfile.WiaRetryOnFailure = CbWiaRetryOnFailure.Checked;
            ScanProfile.WiaDelayBetweenScans = CbWiaDelayBetweenScans.Checked;
            if (double.TryParse(txtWiaDelayBetweenScansSeconds.Text, out double value))
            {
                ScanProfile.WiaDelayBetweenScansSeconds = value;
            }
            ScanProfile.ForcePageSize = CbForcePageSize.Checked;
            ScanProfile.ForcePageSizeCrop = CbForcePageSizeCrop.Checked;
            ScanProfile.FlipDuplexedPages = cbFlipDuplex.Checked;
            if (cmbTwainImpl.SelectedIndex != -1)
            {
                ScanProfile.TwainImpl = (TwainImpl)cmbTwainImpl.SelectedIndex;
            }
            ScanProfile.ExcludeBlankPages = CbExcludeBlankPages.Checked;
            ScanProfile.BlankPageWhiteThreshold = TbWhiteThreshold.Value;
            ScanProfile.BlankPageCoverageThreshold = TbCoverageThreshold.Value;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void TbImageQuality_Scroll(object sender, EventArgs e)
        {
            txtImageQuality.Text = tbImageQuality.Value.ToString("G");
        }

        private void TxtImageQuality_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtImageQuality.Text, out int value))
            {
                if (value >= tbImageQuality.Minimum && value <= tbImageQuality.Maximum)
                {
                    tbImageQuality.Value = value;
                }
            }
        }

        private void CbHighQuality_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void CbExcludeBlankPages_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void CbWiaDelayBetweenScans_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void TbWhiteThreshold_Scroll(object sender, EventArgs e)
        {
            TxtWhiteThreshold.Text = TbWhiteThreshold.Value.ToString("G");
        }

        private void TxtWhiteThreshold_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(TxtWhiteThreshold.Text, out int value))
            {
                if (value >= TbWhiteThreshold.Minimum && value <= TbWhiteThreshold.Maximum)
                {
                    TbWhiteThreshold.Value = value;
                }
            }
        }

        private void TbCoverageThreshold_Scroll(object sender, EventArgs e)
        {
            TxtCoverageThreshold.Text = TbCoverageThreshold.Value.ToString("G");
        }

        private void TxtCoverageThreshold_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(TxtCoverageThreshold.Text, out int value))
            {
                if (value >= TbCoverageThreshold.Minimum && value <= TbCoverageThreshold.Maximum)
                {
                    TbCoverageThreshold.Value = value;
                }
            }
        }

        private void BtnRestoreDefaults_Click(object sender, EventArgs e)
        {
            UpdateValues(appConfigManager.Config.DefaultProfileSettings ?? new ScanProfile { Version = ScanProfile.CURRENT_VERSION });
        }
    }
}