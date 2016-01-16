/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

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
using NAPS2.Scan;
using NAPS2.Scan.Twain;

namespace NAPS2.WinForms
{
    public partial class FAdvancedScanSettings : FormBase
    {
        public FAdvancedScanSettings()
        {
            InitializeComponent();

            cmbTwainImpl.Format += (sender, e) => ((Enum) e.ListItem).Description();
            cmbTwainImpl.Items.Add(TwainImpl.Default);
            cmbTwainImpl.Items.Add(TwainImpl.Legacy);
            if (Environment.Is64BitProcess)
            {
                cmbTwainImpl.Items.Add(TwainImpl.X64);
            }
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            cbHighQuality.Checked = ScanProfile.MaxQuality;
            tbImageQuality.Value = ScanProfile.Quality;
            txtImageQuality.Text = ScanProfile.Quality.ToString("G");
            cbBrightnessContrastAfterScan.Checked = ScanProfile.BrightnessContrastAfterScan;
            cbForcePageSize.Checked = ScanProfile.ForcePageSize;
            cmbTwainImpl.SelectedIndex = (int)ScanProfile.TwainImpl;

            UpdateEnabledControls();

            new LayoutManager(this)
                .Bind(groupBox1, groupBox2, tbImageQuality)
                    .WidthToForm()
                .Bind(txtImageQuality, btnOK, btnCancel)
                    .RightToForm()
                .Activate();
        }

        private void UpdateEnabledControls()
        {
            if (ScanProfile.DriverName != TwainScanDriver.DRIVER_NAME)
            {
                cmbTwainImpl.Enabled = false;
            }
        }

        public ScanProfile ScanProfile { get; set; }

        private void SaveSettings()
        {
            ScanProfile.Quality = tbImageQuality.Value;
            ScanProfile.MaxQuality = cbHighQuality.Checked;
            ScanProfile.BrightnessContrastAfterScan = cbBrightnessContrastAfterScan.Checked;
            ScanProfile.ForcePageSize = cbForcePageSize.Checked;
            ScanProfile.TwainImpl = (TwainImpl)cmbTwainImpl.SelectedIndex;
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
            int value;
            if (int.TryParse(txtImageQuality.Text, out value))
            {
                if (value >= tbImageQuality.Minimum && value <= tbImageQuality.Maximum)
                {
                    tbImageQuality.Value = value;
                }
            }
        }

        private void cbHighQuality_CheckedChanged(object sender, EventArgs e)
        {
            tbImageQuality.Enabled = !cbHighQuality.Checked;
            txtImageQuality.Enabled = !cbHighQuality.Checked;
        }
    }
}
