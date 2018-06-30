using NAPS2.Lang.Resources;
using NAPS2.Scan.Wia;
using System;
using System.IO;
using System.Windows.Forms;
using WIA;

namespace NAPS2.WinForms
{
    public partial class FScanProgress : FormBase
    {
        private bool isComplete;
        private bool cancel;

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
                .Bind(BtnCancel)
                    .RightToForm()
                .Activate();

            LabelPage.Text = string.Format(MiscResources.ScanPageProgress, PageNumber);
        }

        protected override bool ShowWithoutActivation => true;

        private void FScanProgress_Shown(object sender, EventArgs e)
        {
            EventLoop.Do(wia =>
            {
                try
                {
                    var imageFile = (ImageFile)wia.Item?.Transfer(Format);
                    if (imageFile != null)
                    {
                        ImageStream = new MemoryStream((byte[])imageFile.FileData.get_BinaryData());
                    }
                }
                catch (Exception ex)
                {
                    Exception = ex;
                }
                SafeInvoke(() =>
                {
                    DialogResult = cancel ? DialogResult.Cancel : DialogResult.OK;
                    isComplete = true;
                    Close();
                });
            });
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            cancel = true;
            BtnCancel.Enabled = false;
        }

        private void FScanProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel |= !isComplete;
        }
    }
}