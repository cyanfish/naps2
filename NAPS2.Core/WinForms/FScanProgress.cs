using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Scan.Wia;
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

        public WiaBackgroundEventLoop EventLoop { get; set; }

        public string Format { get; set; }

        public Stream ImageStream { get; private set; }

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
            EventLoop.DoAsync(wia =>
            {
                try
                {
                    if (wia.Item != null)
                    {
                        var imageFile = (ImageFile)wia.Item.Transfer(Format);
                        if (imageFile != null)
                        {
                            ImageStream = new MemoryStream((byte[])imageFile.FileData.get_BinaryData());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Exception = ex;
                }
                Invoke(new MethodInvoker(() =>
                {
                    DialogResult = DialogResult.OK;
                    isComplete = true;
                    Close();
                }));
            });
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
