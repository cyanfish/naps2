/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009        Pavel Sorejs
    Copyright (C) 2012, 2013  Ben Olden-Cooligan

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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAPS2.Scan;
using NAPS2.Scan.Driver;
using NAPS2.Scan.Driver.Wia;
using NAPS2.Scan.Driver.Twain;

namespace NAPS2
{
    public partial class FEditScanSettings : Form
    {
        private readonly IScanDriverFactory driverFactory;

        private ScanSettings scanSettings;

        private ScanDevice currentDevice;
        private int iconID;
        private bool result = false;

        private bool suppressChangeEvent = false;

        public bool Result
        {
            get { return result; }
        }

        public ScanSettings ScanSettings
        {
            get { return scanSettings; }
            set { scanSettings = value; }
        }

        public FEditScanSettings(IScanDriverFactory driverFactory)
        {
            InitializeComponent();
            this.driverFactory = driverFactory;
            AddEnumItems<ScanHorizontalAlign>(cmbAlign);
            AddEnumItems<ScanBitDepth>(cmbDepth);
            AddEnumItems<ScanPageSize>(cmbPage);
            AddEnumItems<ScanDPI>(cmbResolution);
            AddEnumItems<ScanScale>(cmbScale);
            AddEnumItems<ScanSource>(cmbSource);
        }

        private void AddEnumItems<T>(ComboBox combo)
        {
            foreach (var item in Enum.GetValues(typeof(T)))
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
            IScanDriver driver = driverFactory.CreateDriver(driverName);
            try
            {
                ScanDevice device = driver.PromptForDevice();
                txtDevice.Text = device.Name;
                currentDevice = device;
            }
            catch (ScanDriverException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnChooseDevice_Click(object sender, EventArgs e)
        {
            try
            {
                if (rdWIA.Checked == true)
                    choose(WiaScanDriver.DRIVER_NAME);
                else
                    choose(TwainScanDriver.DRIVER_NAME);
            }
            catch (Exception)
            {
                MessageBox.Show("An unknown error occured.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveSettings()
        {
            if (rdbNativeWIA.Checked)
            {
                scanSettings = new ScanSettings
                {
                    Device = currentDevice,
                    DisplayName = txtName.Text,
                    IconID = iconID,
                    MaxQuality = cbHighQuality.Checked
                };
            }
            else
            {
                scanSettings = new ExtendedScanSettings
                {
                    Device = currentDevice,
                    DisplayName = txtName.Text,
                    IconID = iconID,
                    MaxQuality = cbHighQuality.Checked,
                    
                    AfterScanScale = (ScanScale)cmbScale.SelectedIndex,
                    BitDepth = (ScanBitDepth)cmbDepth.SelectedIndex,
                    Brightness = trBrightness.Value,
                    Contrast = trContrast.Value,
                    PageAlign = (ScanHorizontalAlign)cmbAlign.SelectedIndex,
                    PageSize = (ScanPageSize)cmbPage.SelectedIndex,
                    Resolution = (ScanDPI)cmbResolution.SelectedIndex,
                    Source = (ScanSource)cmbSource.SelectedIndex
                };
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (currentDevice == null)
            {
                MessageBox.Show("No device selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtName.Text == "")
            {
                MessageBox.Show("Name missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            result = true;
            saveSettings();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rdbNativeWIA_CheckedChanged(object sender, EventArgs e)
        {
            cmbSource.Enabled = rdbConfig.Checked;
            cmbResolution.Enabled = rdbConfig.Checked;
            cmbPage.Enabled = rdbConfig.Checked;
            cmbDepth.Enabled = rdbConfig.Checked;
            cmbAlign.Enabled = rdbConfig.Checked;
            cmbScale.Enabled = rdbConfig.Checked;
            trBrightness.Enabled = rdbConfig.Checked;
            trContrast.Enabled = rdbConfig.Checked;
            txtBrightness.Enabled = rdbConfig.Checked;
            txtContrast.Enabled = rdbConfig.Checked;
        }

        private void FEditScanSettings_Load(object sender, EventArgs e)
        {
            pctIcon.Image = ilProfileIcons.IconsList.Images[ScanSettings.IconID];
            txtName.Text = ScanSettings.DisplayName;
            cmbSource.SelectedIndex = (int)ScanSettings.Source;


            cmbDepth.SelectedIndex = (int)ScanSettings.Depth;
            cmbResolution.SelectedIndex = (int)ScanSettings.Resolution;
            txtContrast.Text = ScanSettings.Contrast.ToString();
            txtBrightness.Text = ScanSettings.Brightness.ToString();
            cmbPage.SelectedIndex = (int)ScanSettings.PageSize;
            cmbScale.SelectedIndex = (int)ScanSettings.AfterScanScale;
            cmbAlign.SelectedIndex = (int)ScanSettings.PageAlign;

            cbHighQuality.Checked = ScanSettings.HighQuality;

            if (ScanSettings.DeviceDriver == ScanSettings.Driver.WIA)
            {
                suppressChangeEvent = true;
                rdWIA.Checked = true;
                suppressChangeEvent = false;
                if (ScanSettings.ShowScanUI)
                    rdbNativeWIA.Checked = true;
                else
                    rdbConfig.Checked = true;
            }
            else
            {
                rdTWAIN.Checked = true;
                rdbNativeWIA.Checked = true;
                rdbNativeWIA.Enabled = rdWIA.Checked;
                rdbConfig.Enabled = rdWIA.Checked;
            }

            if (ScanSettings.DeviceID != "")
            {
                if (ScanSettings.DeviceDriver == ScanSettings.Driver.WIA)
                    loadWIA();
                else
                    loadTWAIN();
            }
        }

        private void pctIcon_DoubleClick(object sender, EventArgs e)
        {
            FChooseIcon fic = new FChooseIcon();
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
                rdbNativeWIA.Checked = true;
                rdbNativeWIA.Enabled = rdWIA.Checked;
                rdbConfig.Enabled = rdWIA.Checked;
                ScanSettings.DeviceID = "";
                currentDeviceID = "";
                txtDevice.Text = "";
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
            txtBrightness.Text = trBrightness.Value.ToString();
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
            txtContrast.Text = trContrast.Value.ToString();
        }
    }
}
