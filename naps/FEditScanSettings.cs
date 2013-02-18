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
using System.Text;
using System.Windows.Forms;
using NAPS2.wia;
using NAPS2.twain;
using NAPS2.Scan;

namespace NAPS2
{
    public partial class FEditScanSettings : Form
    {
        private CScanSettings scanSettings;

        private string currentDeviceID;
        private bool result = false;

        private bool suppressChangeEvent = false;

        public bool Result
        {
            get { return result; }
        }

        public CScanSettings ScanSettings
        {
            get { return scanSettings; }
            set { scanSettings = value; }
        }

        public FEditScanSettings()
        {
            InitializeComponent();

            cmbPage.Items.AddRange(CPageSizes.GetPageSizeList());
        }

        private void chooseWIA()
        {
            string devID;
            try
            {
                devID = CWIAAPI.SelectDeviceUI();
            }
            catch (Exceptions.ENoScannerFound)
            {
                MessageBox.Show("No device found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exceptions.EScannerOffline)
            {
                MessageBox.Show("Device offline.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (devID != null)
            {
                CWIAAPI api = new CWIAAPI(devID);
                txtDevice.Text = api.DeviceName;
                currentDeviceID = devID;
            }
        }

        private void chooseTWAIN()
        {
            Twain tw = new Twain();
            if (!tw.Init(this.Handle))
            {
                MessageBox.Show("No device found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            tw.Select();
            txtDevice.Text = tw.GetCurrentName();
            currentDeviceID = tw.GetCurrentName();
        }

        private void btnChooseDevice_Click(object sender, EventArgs e)
        {
            try
            {
                if (rdWIA.Checked == true)
                    chooseWIA();
                else
                    chooseTWAIN();
            }
            catch (Exception)
            {
                MessageBox.Show("An unknown error occured.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveSettings()
        {
            scanSettings.DeviceID = currentDeviceID;
            scanSettings.Source = (CScanSettings.ScanSource)cmbSource.SelectedIndex;
            scanSettings.ShowScanUI = rdbNativeWIA.Checked;
            scanSettings.Depth = (ScanBitDepth)cmbDepth.SelectedIndex;
            scanSettings.Resolution = (CScanSettings.DPI)cmbResolution.SelectedIndex;
            scanSettings.Brightness = trBrightness.Value;
            scanSettings.Contrast = trContrast.Value;
            scanSettings.DisplayName = txtName.Text;
            scanSettings.PageSize = (CPageSizes.PageSize)cmbPage.SelectedIndex;
            scanSettings.AfterScanScale = (CScanSettings.Scale)cmbScale.SelectedIndex;
            scanSettings.PageAlign = (CScanSettings.HorizontalAlign)cmbAlign.SelectedIndex;
            scanSettings.DeviceDriver = rdWIA.Checked ? CScanSettings.Driver.WIA : CScanSettings.Driver.TWAIN;
            scanSettings.HighQuality = cbHighQuality.Checked;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (currentDeviceID == null || currentDeviceID == "")
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
        }

        private void loadWIA()
        {
            currentDeviceID = ScanSettings.DeviceID;
            CWIAAPI api;
            try
            {
                api = new CWIAAPI(ScanSettings.DeviceID);
            }
            catch (Exceptions.EScannerNotFound)
            {
                MessageBox.Show("Device not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ScanSettings.DeviceID = "";
                currentDeviceID = "";
                txtDevice.Text = "";
                return;
            }
            txtDevice.Text = api.DeviceName;
        }

        private void loadTWAIN()
        {
            Twain tw = new Twain();
            if (!tw.Init(this.Handle))
            {
                MessageBox.Show("No device found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ScanSettings.DeviceID = "";
                currentDeviceID = "";
                txtDevice.Text = "";
                return;
            }
            if (!tw.SelectByName(ScanSettings.DeviceID))
            {
                MessageBox.Show("Device not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ScanSettings.DeviceID = "";
                currentDeviceID = "";
                txtDevice.Text = "";
                return;
            }
            txtDevice.Text = tw.GetCurrentName();
            currentDeviceID = tw.GetCurrentName();
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

            if (ScanSettings.DeviceDriver == CScanSettings.Driver.WIA)
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
                if (ScanSettings.DeviceDriver == CScanSettings.Driver.WIA)
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
