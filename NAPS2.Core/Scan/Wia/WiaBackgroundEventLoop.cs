using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NAPS2.Scan.Images;
using WIA;

namespace NAPS2.Scan.Wia
{
    // Because WIA.Item.Transfer blocks the UI thread, we need a second event loop to avoid that.
    // Bit of a pain but it works.
    public class WiaBackgroundEventLoop : IDisposable
    {
        private readonly ExtendedScanSettings settings;
        private readonly ScanDevice scanDevice;
        private readonly IScannedImageFactory scannedImageFactory;

        private Thread thread;
        private AutoResetEvent initReset = new AutoResetEvent(false);
        private Form form;

        public WiaBackgroundEventLoop(ExtendedScanSettings settings, ScanDevice scanDevice, IScannedImageFactory scannedImageFactory)
        {
            this.settings = settings;
            this.scanDevice = scanDevice;
            this.scannedImageFactory = scannedImageFactory;

            Start();
        }

        public IDevice WiaDevice { get; private set; }

        public IItem WiaItem { get; private set; }

        public WiaApi Api { get; private set; }

        public void Start()
        {
            if (thread == null)
            {
                thread = new Thread(RunEventLoop);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                initReset.WaitOne();
            }
        }

        public void Do(Action action)
        {
            form.BeginInvoke(action);
        }

        public void Sync()
        {
            var reset = new AutoResetEvent(false);
            Do(() => reset.Set());
            reset.WaitOne();
        }

        private void RunEventLoop()
        {
            form = new Form();
            form.Load += form_Load;
            Application.Run(form);
        }

        private void form_Load(object sender, EventArgs e)
        {
            Api = new WiaApi(settings, scanDevice, scannedImageFactory);
            WiaDevice = Api.Device;
            WiaItem = Api.Item;
            initReset.Set();
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

        public void Dispose()
        {
            if (thread != null)
            {
                Do(Application.ExitThread);
            }
        }
    }
}
