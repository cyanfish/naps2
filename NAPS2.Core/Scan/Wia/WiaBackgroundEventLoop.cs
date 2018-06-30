using NAPS2.Util;
using System;
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

        public WiaBackgroundEventLoop(ScanProfile profile, ScanDevice scanDevice, ThreadFactory threadFactory)
        {
            this.profile = profile;
            this.scanDevice = scanDevice;

            thread = threadFactory.CreateThread(RunEventLoop);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            // Wait for the thread to initialize the background form and event loop
            initWaiter.WaitOne();
        }

        public void DoSync(Action<WiaState> action)
        {
            Exception error = null;
            form.Invoke(new Action(() =>
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

        public T GetSync<T>(Func<WiaState, T> action)
        {
            T value = default(T);
            // TODO: https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1021.md
            DoSync(wia => value = action(wia));
            return value;
        }

        public void Do(Action<WiaState> action)
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

        #region IDisposable Support

        private bool disposed; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (thread != null)
                    {
                        try
                        {
                            initWaiter.Dispose();
                            form.Invoke(new Action(Application.ExitThread));
                        }
                        catch (Exception ex)
                        {
                            Log.ErrorException("Error disposing WIA event loop", ex);
                        }
                        thread = null;
                    }
                }
                disposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose() => Dispose(true);

        public void Dispose() => Dispose(true);

        #endregion IDisposable Support
    }
}