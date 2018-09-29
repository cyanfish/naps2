using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.Logging;
using NAPS2.Util;

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

        public void DoSync(Action<WiaState> action, Form invokeForm)
        {
            Exception error = null;
            (invokeForm ?? form).Invoke(new Action(() =>
            {
                try
                {
                    if (wiaState == null)
                    {
                        wiaState = InitWia();
                    }
                    action(wiaState);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }));
            if (error != null)
            {
                WiaApi.ThrowDeviceError(error);
            }
        }

        public T GetSync<T>(Func<WiaState, T> action, Form invokeForm = null)
        {
            T value = default;
            DoSync(wia =>
            {
                value = action(wia);
            }, invokeForm);
            return value;
        }

        public void DoAsync(Action<WiaState> action)
        {
            form.BeginInvoke(new Action(() =>
            {
                if (wiaState == null)
                {
                    wiaState = InitWia();
                }
                action(wiaState);
            }));
        }

        public void Dispose()
        {
            if (thread != null)
            {
                try
                {
                    form.Invoke(new Action(Application.ExitThread));
                }
                catch (Exception ex)
                {
                    Log.ErrorException("Error disposing WIA event loop", ex);
                }
                thread = null;
            }
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
