/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;
using Ninject;
using NLog;

namespace NAPS2
{
    public partial class FEditScanSettings : Form
    {
        private readonly Logger logger;
        private readonly IErrorOutput errorOutput;

        private ScanDevice currentDevice;

        private int iconID;
        private bool result;

        private bool suppressChangeEvent;

        public FEditScanSettings(Logger logger, IErrorOutput errorOutput)
        {
            this.logger = logger;
            this.errorOutput = errorOutput;
            InitializeComponent();
            AddEnumItems<ScanHorizontalAlign>(cmbAlign);
            AddEnumItems<ScanBitDepth>(cmbDepth);
            AddEnumItems<ScanPageSize>(cmbPage);
            AddEnumItems<ScanDpi>(cmbResolution);
            AddEnumItems<ScanScale>(cmbScale);
            AddEnumItems<ScanSource>(cmbSource);
        }

        public bool Result
        {
            get { return result; }
        }

        public ScanSettings ScanSettings { get; set; }

        private string DeviceDriverName
        {
            get
            {
                return rdTWAIN.Checked ? TwainScanDriver.DRIVER_NAME : WiaScanDriver.DRIVER_NAME;
            }
            set
            {
                if (value == TwainScanDriver.DRIVER_NAME)
                {
                    rdTWAIN.Checked = true;
                }
                else
                {
                    rdWIA.Checked = true;
                }
            }
        }

        public ScanDevice CurrentDevice
        {
            get { return currentDevice; }
            set
            {
                currentDevice = value;
                txtDevice.Text = (value == null ? "" : value.Name);
            }
        }

        private void AddEnumItems<T>(ComboBox combo)
        {
            foreach (object item in Enum.GetValues(typeof(T)))
            {
                combo.Items.Add(item);
            }
            combo.Format += Combo_Format;
        }

        void Combo_Format(object sender, ListControlConvertEventArgs e)
        {
            e.Value = ((Enum)e.ListItem).Description();
        }

        private void choose(string driverName)
        {
            var driver = KernelManager.Kernel.Get<IScanDriver>(driverName);
            try
            {
                driver.DialogParent = this;
                ScanDevice device = driver.PromptForDevice();
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
                    logger.ErrorException(e.Message, e.InnerException);
                }
                errorOutput.DisplayError(e.Message);
            }
        }

        private void btnChooseDevice_Click(object sender, EventArgs e)
        {
            choose(DeviceDriverName);
        }

        private void saveSettings()
        {
            if (rdbNativeWIA.Checked)
            {
                ScanSettings = new ScanSettings
                {
                    Device = CurrentDevice,
                    DisplayName = txtName.Text,
                    IconID = iconID,
                    MaxQuality = cbHighQuality.Checked
                };
            }
            else
            {
                ScanSettings = new ExtendedScanSettings
                {
                    Device = CurrentDevice,
                    DisplayName = txtName.Text,
                    IconID = iconID,
                    MaxQuality = cbHighQuality.Checked,

                    AfterScanScale = (ScanScale)cmbScale.SelectedIndex,
                    BitDepth = (ScanBitDepth)cmbDepth.SelectedIndex,
                    Brightness = trBrightness.Value,
                    Contrast = trContrast.Value,
                    PageAlign = (ScanHorizontalAlign)cmbAlign.SelectedIndex,
                    PageSize = (ScanPageSize)cmbPage.SelectedIndex,
                    Resolution = (ScanDpi)cmbResolution.SelectedIndex,
                    PaperSource = (ScanSource)cmbSource.SelectedIndex
                };
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (CurrentDevice == null)
            {
                errorOutput.DisplayError(MiscResources.NoDeviceSelected);
                return;
            }

            if (txtName.Text == "")
            {
                errorOutput.DisplayError(MiscResources.NameMissing);
                return;
            }
            result = true;
            saveSettings();
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
            if (!suppressChangeEvent)
            {
                suppressChangeEvent = true;
                bool configEnabled = rdWIA.Checked;
                rdbConfig.Enabled = configEnabled;
                rdbNativeWIA.Enabled = configEnabled;
                bool enabled = rdbConfig.Checked && rdWIA.Checked;
                cmbSource.Enabled = enabled;
                cmbResolution.Enabled = enabled;
                cmbPage.Enabled = enabled;
                cmbDepth.Enabled = enabled;
                cmbAlign.Enabled = enabled;
                cmbScale.Enabled = enabled;
                trBrightness.Enabled = enabled;
                trContrast.Enabled = enabled;
                txtBrightness.Enabled = enabled;
                txtContrast.Enabled = enabled;
                suppressChangeEvent = false;
            }
        }

        private void FEditScanSettings_Load(object sender, EventArgs e)
        {
            pctIcon.Image = ilProfileIcons.IconsList.Images[ScanSettings.IconID];
            txtName.Text = ScanSettings.DisplayName;
            CurrentDevice = ScanSettings.Device;
            iconID = ScanSettings.IconID;

            var scanSettingsExt = ScanSettings as ExtendedScanSettings;

            if (scanSettingsExt != null)
            {
                cmbSource.SelectedIndex = (int)scanSettingsExt.PaperSource;
                cmbDepth.SelectedIndex = (int)scanSettingsExt.BitDepth;
                cmbResolution.SelectedIndex = (int)scanSettingsExt.Resolution;
                txtContrast.Text = scanSettingsExt.Contrast.ToString("G");
                txtBrightness.Text = scanSettingsExt.Brightness.ToString("G");
                cmbPage.SelectedIndex = (int)scanSettingsExt.PageSize;
                cmbScale.SelectedIndex = (int)scanSettingsExt.AfterScanScale;
                cmbAlign.SelectedIndex = (int)scanSettingsExt.PageAlign;
            }

            cbHighQuality.Checked = ScanSettings.MaxQuality;

            if (CurrentDevice == null || CurrentDevice.DriverName != TwainScanDriver.DRIVER_NAME)
            {
                suppressChangeEvent = true;
                rdWIA.Checked = true;
                suppressChangeEvent = false;
                if (scanSettingsExt == null)
                    rdbNativeWIA.Checked = true;
                else
                    rdbConfig.Checked = true;
            }
            else
            {
                rdTWAIN.Checked = true;
                rdbNativeWIA.Checked = true;
            }

            UpdateEnabledControls();
        }

        private void pctIcon_DoubleClick(object sender, EventArgs e)
        {
            var fic = new FChooseIcon();
            fic.ShowDialog();
            if (fic.IconID > -1)
            {
                pctIcon.Image = ilProfileIcons.IconsList.Images[fic.IconID];
                ScanSettings.IconID = fic.IconID;
            }
        }

        private void rdWIA_CheckedChanged(object sender, EventArgs e)
        {
            if (!suppressChangeEvent)
            {
                ScanSettings.Device = null;
                CurrentDevice = null;
                UpdateEnabledControls();
            }
        }

        private void txtBrightness_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(txtBrightness.Text, out value))
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
            int value;
            if (int.TryParse(txtContrast.Text, out value))
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
    }
}
