using NAPS2.Scan;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FSelectDevice : FormBase
    {
        public FSelectDevice()
        {
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = BtnSelect;
            CancelButton = BtnCancel;
        }

        public List<ScanDevice> DeviceList { get; set; }

        public ScanDevice SelectedDevice { get; private set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(ListboxDevices)
                    .WidthToForm()
                    .HeightToForm()
                .Bind(BtnSelect, BtnCancel)
                    .RightToForm()
                .Activate();

            foreach (var device in DeviceList)
            {
                ListboxDevices.Items.Add(device);
            }
            if (ListboxDevices.Items.Count > 0)
            {
                ListboxDevices.SelectedIndex = 0;
            }
        }

        private void ListboxDevices_Format(object sender, ListControlConvertEventArgs e)
        {
            e.Value = ((ScanDevice)e.ListItem).Name;
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            if (ListboxDevices.SelectedItem == null)
            {
                ListboxDevices.Focus();
                return;
            }
            DialogResult = DialogResult.OK;
            SelectedDevice = ((ScanDevice)ListboxDevices.SelectedItem);
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}