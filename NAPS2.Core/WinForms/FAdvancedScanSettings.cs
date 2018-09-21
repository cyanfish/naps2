using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;
using NAPS2.Scan;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;

namespace NAPS2.WinForms
{
    public partial class FAdvancedScanSettings : FormBase
    {
        private readonly AppConfigManager appConfigManager;

        public FAdvancedScanSettings(AppConfigManager appConfigManager)
        {
            this.appConfigManager = appConfigManager;
            InitializeComponent();
            txtWiaDelayBetweenScansSeconds.Left = cbWiaDelayBetweenScans.Right + 6;

            cmbTwainImpl.Format += (sender, e) => e.Value = ((Enum) e.ListItem).Description();
            cmbTwainImpl.Items.Add(TwainImpl.Default);
            cmbTwainImpl.Items.Add(TwainImpl.MemXfer);
            cmbTwainImpl.Items.Add(TwainImpl.OldDsm);
            cmbTwainImpl.Items.Add(TwainImpl.Legacy);
            if (Environment.Is64BitProcess)
            {
                cmbTwainImpl.Items.Add(TwainImpl.X64);
            }
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            UpdateValues(ScanProfile);
            UpdateEnabled();

            new LayoutManager(this)
                .Bind(groupBox1, groupBox2, groupBox3, tbImageQuality, tbWhiteThreshold, tbCoverageThreshold)
                    .WidthToForm()
                .Bind(txtImageQuality, txtWhiteThreshold, txtCoverageThreshold, btnOK, btnCancel)
                    .RightToForm()
                .Activate();
        }

        private void UpdateValues(ScanProfile scanProfile)
        {
            cbHighQuality.Checked = scanProfile.MaxQuality;
            tbImageQuality.Value = scanProfile.Quality;
            txtImageQuality.Text = scanProfile.Quality.ToString("G");
            cbBrightnessContrastAfterScan.Checked = scanProfile.BrightnessContrastAfterScan;
            cbAutoDeskew.Checked = scanProfile.AutoDeskew;
            cbWiaOffsetWidth.Checked = scanProfile.WiaOffsetWidth;
            cbWiaRetryOnFailure.Checked = scanProfile.WiaRetryOnFailure;
            cbWiaDelayBetweenScans.Checked = scanProfile.WiaDelayBetweenScans;
            txtWiaDelayBetweenScansSeconds.Text = scanProfile.WiaDelayBetweenScansSeconds.ToString("G");
            cbForcePageSize.Checked = scanProfile.ForcePageSize;
            cbForcePageSizeCrop.Checked = scanProfile.ForcePageSizeCrop;
            cbFlipDuplex.Checked = scanProfile.FlipDuplexedPages;
            cmbTwainImpl.SelectedIndex = (int) scanProfile.TwainImpl;
            cbExcludeBlankPages.Checked = scanProfile.ExcludeBlankPages;
            tbWhiteThreshold.Value = scanProfile.BlankPageWhiteThreshold;
            txtWhiteThreshold.Text = scanProfile.BlankPageWhiteThreshold.ToString("G");
            tbCoverageThreshold.Value = scanProfile.BlankPageCoverageThreshold;
            txtCoverageThreshold.Text = scanProfile.BlankPageCoverageThreshold.ToString("G");
        }

        private void UpdateEnabled()
        {
            cmbTwainImpl.Enabled = ScanProfile.DriverName == TwainScanDriver.DRIVER_NAME;
            cbWiaOffsetWidth.Enabled = ScanProfile.DriverName == WiaScanDriver.DRIVER_NAME;
            cbWiaRetryOnFailure.Enabled = ScanProfile.DriverName == WiaScanDriver.DRIVER_NAME;
            cbWiaDelayBetweenScans.Enabled = ScanProfile.DriverName == WiaScanDriver.DRIVER_NAME;
            txtWiaDelayBetweenScansSeconds.Enabled = ScanProfile.DriverName == WiaScanDriver.DRIVER_NAME && cbWiaDelayBetweenScans.Checked;
            tbImageQuality.Enabled = !cbHighQuality.Checked;
            txtImageQuality.Enabled = !cbHighQuality.Checked;
            tbWhiteThreshold.Enabled = cbExcludeBlankPages.Checked && ScanProfile.BitDepth != ScanBitDepth.BlackWhite;
            txtWhiteThreshold.Enabled = cbExcludeBlankPages.Checked && ScanProfile.BitDepth != ScanBitDepth.BlackWhite;
            tbCoverageThreshold.Enabled = cbExcludeBlankPages.Checked;
            txtCoverageThreshold.Enabled = cbExcludeBlankPages.Checked;
        }

        public ScanProfile ScanProfile { get; set; }

        private void SaveSettings()
        {
            ScanProfile.Quality = tbImageQuality.Value;
            ScanProfile.MaxQuality = cbHighQuality.Checked;
            ScanProfile.BrightnessContrastAfterScan = cbBrightnessContrastAfterScan.Checked;
            ScanProfile.AutoDeskew = cbAutoDeskew.Checked;
            ScanProfile.WiaOffsetWidth = cbWiaOffsetWidth.Checked;
            ScanProfile.WiaRetryOnFailure = cbWiaRetryOnFailure.Checked;
            ScanProfile.WiaDelayBetweenScans = cbWiaDelayBetweenScans.Checked;
            if (double.TryParse(txtWiaDelayBetweenScansSeconds.Text, out double value))
            {
                ScanProfile.WiaDelayBetweenScansSeconds = value;
            }
            ScanProfile.ForcePageSize = cbForcePageSize.Checked;
            ScanProfile.ForcePageSizeCrop = cbForcePageSizeCrop.Checked;
            ScanProfile.FlipDuplexedPages = cbFlipDuplex.Checked;
            if (cmbTwainImpl.SelectedIndex != -1)
            {
                ScanProfile.TwainImpl = (TwainImpl) cmbTwainImpl.SelectedIndex;
            }
            ScanProfile.ExcludeBlankPages = cbExcludeBlankPages.Checked;
            ScanProfile.BlankPageWhiteThreshold = tbWhiteThreshold.Value;
            ScanProfile.BlankPageCoverageThreshold = tbCoverageThreshold.Value;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tbImageQuality_Scroll(object sender, EventArgs e)
        {
            txtImageQuality.Text = tbImageQuality.Value.ToString("G");
        }

        private void txtImageQuality_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtImageQuality.Text, out int value))
            {
                if (value >= tbImageQuality.Minimum && value <= tbImageQuality.Maximum)
                {
                    tbImageQuality.Value = value;
                }
            }
        }

        private void cbHighQuality_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void cbExcludeBlankPages_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void cbWiaDelayBetweenScans_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void tbWhiteThreshold_Scroll(object sender, EventArgs e)
        {
            txtWhiteThreshold.Text = tbWhiteThreshold.Value.ToString("G");
        }

        private void txtWhiteThreshold_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtWhiteThreshold.Text, out int value))
            {
                if (value >= tbWhiteThreshold.Minimum && value <= tbWhiteThreshold.Maximum)
                {
                    tbWhiteThreshold.Value = value;
                }
            }
        }

        private void tbCoverageThreshold_Scroll(object sender, EventArgs e)
        {
            txtCoverageThreshold.Text = tbCoverageThreshold.Value.ToString("G");
        }

        private void txtCoverageThreshold_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtCoverageThreshold.Text, out int value))
            {
                if (value >= tbCoverageThreshold.Minimum && value <= tbCoverageThreshold.Maximum)
                {
                    tbCoverageThreshold.Value = value;
                }
            }
        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            UpdateValues(appConfigManager.Config.DefaultProfileSettings ?? new ScanProfile { Version = ScanProfile.CURRENT_VERSION });
        }
    }
}
