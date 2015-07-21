using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using WIA;

namespace NAPS2.WinForms
{
    public partial class FScanProgress : FormBase
    {
        private bool isComplete;

        public FScanProgress()
        {
            InitializeComponent();
        }

        public int PageNumber { get; set; }

        public Item Item { get; set; }

        public string Format { get; set; }

        public ImageFile ImageFile { get; private set; }

        public Exception Exception { get; private set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(progressBar)
                    .WidthToForm()
                .Bind(btnCancel)
                    .RightToForm()
                .Activate();

            labelPage.Text = string.Format(MiscResources.ScanPageProgress, PageNumber);
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        private void FScanProgress_Shown(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    ImageFile = (ImageFile)Item.Transfer(Format);
                }
                catch (Exception ex)
                {
                    Exception = ex;
                }
            }).ContinueWith(task =>
            {
                DialogResult = DialogResult.OK;
                isComplete = true;
                Close();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void FScanProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isComplete)
            {
                // Prevent simultaneous transfers by refusing to close until the transfer is complete
                e.Cancel = true;
            }
        }
    }
}
