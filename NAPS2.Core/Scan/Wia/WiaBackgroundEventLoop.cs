using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace NAPS2.Scan.Wia
{
    /// <summary>
    /// Manages a separate Windows Forms event loop to allow WIA interaction to be performed asynchronously.
    /// </summary>
    public class WiaBackgroundEventLoop : IDisposable
    {
        private readonly ScanProfile profile;
        private readonly ScanDevice scanDevice;

        private readonly AutoResetEvent initWaiter = new AutoResetEvent(false);
        private Thread thread;
        private Form form;
        private WiaState wiaState;

        public WiaBackgroundEventLoop(ScanProfile profile, ScanDevice scanDevice)
        {
            this.profile = profile;
            this.scanDevice = scanDevice;

            thread = new Thread(RunEventLoop);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            // Wait for the thread to initialize the background form and event loop
            initWaiter.WaitOne();
        }

        public void DoSync(Action<WiaState> action)
        {
            form.Invoke(Bind(action));
        }

        public T GetSync<T>(Func<WiaState, T> action)
        {
            T value = default(T);
            form.Invoke(Bind(wia =>
            {
                value = action(wia);
            }));
            return value;
        }

        public void DoAsync(Action<WiaState> action)
        {
            form.BeginInvoke(Bind(action));
        }

        public void Dispose()
        {
            if (thread != null)
            {
                DoSync(wia => Application.ExitThread());
                thread = null;
            }
        }

        private Action Bind(Action<WiaState> action)
        {
            return () =>
            {
                if (wiaState == null)
                {
                    wiaState = InitWia();
                }
                action(wiaState);
            };
        }

        private WiaState InitWia()
        {
            var device = WiaApi.GetDevice(scanDevice);
            var item = WiaApi.GetItem(device, profile);
            WiaApi.Configure(device, item, profile);
            return new WiaState(device, item);
        }

        private void RunEventLoop()
        {
            form = new Form
            {
                WindowState = FormWindowState.Minimized,
                ShowInTaskbar = false
            };
            form.Load += (sender, e) => initWaiter.Set();
            Application.Run(form);
        }
    }
}
