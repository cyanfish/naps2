using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FBatchPrompt : FormBase
    {
        public FBatchPrompt()
        {
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = btnScan;
        }

        public int ScanNumber { get; set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            lblStatus.Text = string.Format(lblStatus.Text, ScanNumber);

            new LayoutManager(this)
                .Bind(btnScan, btnDone)
                    .WidthTo(() => Width / 2)
                .Bind(btnDone)
                    .LeftTo(() => btnScan.Right);

            Activate();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
