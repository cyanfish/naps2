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
            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }

        public List<ScanDevice> DeviceList { get; set; } 

        public ScanDevice SelectedDevice { get; private set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
