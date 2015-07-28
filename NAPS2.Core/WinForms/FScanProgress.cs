using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

        public string DeviceID { get; set; }

        public string ItemID { get; set; }

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
            var eventLoop = new InternalEventLoop(() =>
            {
                try
                {
                    var deviceManager = new DeviceManagerClass();
                    var deviceInfo = deviceManager.DeviceInfos.Cast<DeviceInfo>().First(x => x.DeviceID == DeviceID);
                    var device = deviceInfo.Connect();
                    var item = device.GetItem(ItemID);
                    var imageFile = (ImageFile)item.Transfer(Format);
                    if (imageFile != null)
                    {
                        ImageStream = new MemoryStream((byte[])imageFile.FileData.get_BinaryData());
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
            eventLoop.Start();
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

        // Because WIA.Item.Transfer blocks the UI thread, we need a second event loop to avoid that.
        // Bit of a pain but it works.
        private class InternalEventLoop
        {
            private readonly Thread thread;
            private readonly Action action;

            public InternalEventLoop(Action action)
            {
                this.action = action;
                thread = new Thread(RunEventLoop);
                thread.SetApartmentState(ApartmentState.STA);
            }

            public void Start()
            {
                thread.Start();
            }

            private void RunEventLoop()
            {
                var form = new Form();
                form.Load += DoAction;
                Application.Run(form);
            }

            private void DoAction(object sender, EventArgs e)
            {
                try
                {
                    action();
                }
                finally
                {
                    Application.ExitThread();
                }
            }

            private class InvisibleForm : Form
            {
                protected override void SetVisibleCore(bool value)
                {
                    if (!IsHandleCreated)
                    {
                        CreateHandle();
                        value = false;
                    }
                    base.SetVisibleCore(value);
                }
            }
        }
    }
}
