/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2014  Ben Olden-Cooligan

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

namespace NAPS2.WinForms
{
    public partial class FEditScanSettings : FormBase
    {
        private readonly IScanDriverFactory driverFactory;
        private readonly IErrorOutput errorOutput;

        private ScanDevice currentDevice;
        private bool isDefault;

        private int iconID;
        private bool result;

        private bool suppressChangeEvent;

        public FEditScanSettings(IScanDriverFactory driverFactory, IErrorOutput errorOutput)
        {
            this.driverFactory = driverFactory;
            this.errorOutput = errorOutput;
            InitializeComponent();
            AddEnumItems<ScanHorizontalAlign>(cmbAlign);
            AddEnumItems<ScanBitDepth>(cmbDepth);
            AddEnumItems<ScanPageSize>(cmbPage, FormatPageSize);
            AddEnumItems<ScanDpi>(cmbResolution);
            AddEnumItems<ScanScale>(cmbScale);
            AddEnumItems<ScanSource>(cmbSource);
        }

        private void FormatPageSize(object sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is PageDimensions)
            {
                var pageDimensions = (PageDimensions)e.ListItem;
                e.Value = string.Format(MiscResources.CustomPageSizeFormat, pageDimensions.Width, pageDimensions.Height, pageDimensions.Unit.Description());
            }
            else
            {
                e.Value = ((Enum)e.ListItem).Description();
            }
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            // Don't trigger any onChange events
            suppressChangeEvent = true;

            pctIcon.Image = ilProfileIcons.IconsList.Images[ScanSettings.IconID];
            txtName.Text = ScanSettings.DisplayName;
            CurrentDevice = ScanSettings.Device;
            isDefault = ScanSettings.IsDefault;
            iconID = ScanSettings.IconID;

            cmbSource.SelectedIndex = (int)ScanSettings.PaperSource;
            cmbDepth.SelectedIndex = (int)ScanSettings.BitDepth;
            cmbResolution.SelectedIndex = (int)ScanSettings.Resolution;
            txtContrast.Text = ScanSettings.Contrast.ToString("G");
            txtBrightness.Text = ScanSettings.Brightness.ToString("G");
            if (ScanSettings.PageSize == ScanPageSize.Custom)
            {
                cmbPage.Items.Add(ScanSettings.CustomPageSize);
                cmbPage.SelectedIndex = (int)ScanPageSize.Custom + 1;
            }
            else
            {
                cmbPage.SelectedIndex = (int)ScanSettings.PageSize;
            }
            cmbScale.SelectedIndex = (int)ScanSettings.AfterScanScale;
            cmbAlign.SelectedIndex = (int)ScanSettings.PageAlign;

            cbHighQuality.Checked = ScanSettings.MaxQuality;

            // The setter updates the driver selection checkboxes
            DeviceDriverName = ScanSettings.DriverName;

            rdbNative.Checked = ScanSettings.UseNativeUI;
            rdbConfig.Checked = !ScanSettings.UseNativeUI;

            // Start triggering onChange events again
            suppressChangeEvent = false;

            UpdateEnabledControls();

            new LayoutManager(this)
                .Bind(txtName, txtDevice, panel1, panel2)
                    .WidthToForm()
                .Bind(pctIcon, btnChooseDevice, btnOK, btnCancel)
                    .RightToForm()
                .Bind(cmbAlign, cmbDepth, cmbPage, cmbResolution, cmbScale, cmbSource, trBrightness, trContrast)
                    .WidthTo(() => Width / 2)
                .Bind(rdTWAIN, rdbNative, label3, cmbDepth, label9, cmbAlign, label10, cmbScale, label7, trContrast)
                    .LeftTo(() => Width / 2)
                .Bind(txtBrightness)
                    .LeftTo(() => trBrightness.Right)
                .Bind(txtContrast)
                    .LeftTo(() => trContrast.Right)
                .Activate();
        }

        public bool Result
        {
            get { return result; }
        }

        public ExtendedScanSettings ScanSettings { get; set; }

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

        private void ChooseDevice(string driverName)
        {
            var driver = driverFactory.Create(driverName);
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
                    Log.ErrorException(e.Message, e.InnerException);
                }
                errorOutput.DisplayError(e.Message);
            }
        }

        private void btnChooseDevice_Click(object sender, EventArgs e)
        {
            ChooseDevice(DeviceDriverName);
        }

        private void SaveSettings()
        {
            ScanPageSize pageSize;
            PageDimensions customPageSize = null;
            if (cmbPage.SelectedIndex > (int)ScanPageSize.Custom)
            {
                pageSize = ScanPageSize.Custom;
                customPageSize = (PageDimensions)cmbPage.SelectedItem;
            }
            else if (cmbPage.SelectedIndex == (int)ScanPageSize.Custom)
            {
                throw new InvalidOperationException("Custom page size should never be selected when saving");
            }
            else
            {
                pageSize = (ScanPageSize)cmbPage.SelectedIndex;
            }
            ScanSettings = new ExtendedScanSettings
            {
                Version = ExtendedScanSettings.CURRENT_VERSION,

                Device = CurrentDevice,
                IsDefault = isDefault,
                DriverName = DeviceDriverName,
                DisplayName = txtName.Text,
                IconID = iconID,
                MaxQuality = cbHighQuality.Checked,
                UseNativeUI = rdbNative.Checked,

                AfterScanScale = (ScanScale)cmbScale.SelectedIndex,
                BitDepth = (ScanBitDepth)cmbDepth.SelectedIndex,
                Brightness = trBrightness.Value,
                Contrast = trContrast.Value,
                PageAlign = (ScanHorizontalAlign)cmbAlign.SelectedIndex,
                PageSize = pageSize,
                CustomPageSize = customPageSize,
                Resolution = (ScanDpi)cmbResolution.SelectedIndex,
                PaperSource = (ScanSource)cmbSource.SelectedIndex
            };
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Note: If CurrentDevice is null, that's fine. A prompt will be shown when scanning.

            if (txtName.Text == "")
            {
                errorOutput.DisplayError(MiscResources.NameMissing);
                return;
            }
            result = true;
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
            if (!suppressChangeEvent)
            {
                suppressChangeEvent = true;
                bool enabled = rdbConfig.Checked;
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

        private void cmbPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPage.SelectedIndex == (int)ScanPageSize.Custom)
            {
                var form = FormFactory.Create<FPageSize>();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (cmbPage.Items.Count > (int)ScanPageSize.Custom + 1)
                    {
                        cmbPage.Items.RemoveAt(cmbPage.Items.Count - 1);
                    }
                    cmbPage.Items.Add(form.Result);
                    cmbPage.SelectedIndex = (int)ScanPageSize.Custom + 1;
                }
                else
                {
                    if (ScanSettings.PageSize == ScanPageSize.Custom)
                    {
                        cmbPage.SelectedIndex = (int)ScanPageSize.Custom + 1;
                    }
                    else
                    {
                        cmbPage.SelectedIndex = (int)ScanSettings.PageSize;
                    }
                }
            }
        }
    }
}
