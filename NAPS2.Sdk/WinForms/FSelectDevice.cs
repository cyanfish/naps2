using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan;

namespace NAPS2.WinForms
{
    public partial class FSelectDevice : FormBase
    {
        public FSelectDevice()
        {
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = btnSelect;
            CancelButton = btnCancel;
        }

        public List<ScanDevice> DeviceList { get; set; } 

        public ScanDevice SelectedDevice { get; private set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(listboxDevices)
                    .WidthToForm()
                    .HeightToForm()
                .Bind(btnSelect, btnCancel)
                    .RightToForm()
                .Activate();

            foreach (var device in DeviceList)
            {
                listboxDevices.Items.Add(device);
            }
            if (listboxDevices.Items.Count > 0)
            {
                listboxDevices.SelectedIndex = 0;
            }
        }

        private void listboxDevices_Format(object sender, ListControlConvertEventArgs e)
        {
            e.Value = ((ScanDevice)e.ListItem).Name;
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (listboxDevices.SelectedItem == null)
            {
                listboxDevices.Focus();
                return;
            }
            DialogResult = DialogResult.OK;
            SelectedDevice = ((ScanDevice) listboxDevices.SelectedItem);
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
