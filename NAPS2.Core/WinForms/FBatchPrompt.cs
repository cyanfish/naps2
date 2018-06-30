using System;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FBatchPrompt : FormBase
    {
        public FBatchPrompt()
        {
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = BtnScan;
        }

        public int ScanNumber { get; set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            lblStatus.Text = string.Format(lblStatus.Text, ScanNumber);

            new LayoutManager(this)
                .Bind(BtnScan, BtnDone)
                    .WidthTo(() => Width / 2)
                .Bind(BtnDone)
                    .LeftTo(() => BtnScan.Right);

            Activate();
        }

        private void BtnScan_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnDone_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}