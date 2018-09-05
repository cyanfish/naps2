using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Platform;
using NAPS2.Recovery;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.WinForms;
using NAPS2.Worker;

namespace NAPS2.Scan.Twain
{
    public class TwainScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "twain";
        
        private readonly IWorkerServiceFactory workerServiceFactory;
        private readonly TwainWrapper twainWrapper;
        private readonly IFormFactory formFactory;

        public TwainScanDriver(IWorkerServiceFactory workerServiceFactory, TwainWrapper twainWrapper, IFormFactory formFactory)
        {
            this.workerServiceFactory = workerServiceFactory;
            this.twainWrapper = twainWrapper;
            this.formFactory = formFactory;
        }

        public override string DriverName => DRIVER_NAME;

        public override bool IsSupported => PlatformCompat.System.IsTwainDriverSupported;
        
        private bool UseWorker => ScanProfile.TwainImpl != TwainImpl.X64 && Environment.Is64BitProcess && PlatformCompat.Runtime.UseWorker;

        protected override ScanDevice PromptForDeviceInternal()
        {
            var deviceList = GetDeviceList();

            if (!deviceList.Any())
            {
                throw new NoDevicesFoundException();
            }

            var form = formFactory.Create<FSelectDevice>();
            form.DeviceList = deviceList;
            form.ShowDialog();
            return form.SelectedDevice;
        }

        protected override List<ScanDevice> GetDeviceListInternal()
        {
            // Exclude WIA proxy devices since NAPS2 already supports WIA
            return GetFullDeviceList().Where(x => !x.ID.StartsWith("WIA-", StringComparison.InvariantCulture)).ToList();
        }

        private IEnumerable<ScanDevice> GetFullDeviceList()
        {
            var twainImpl = ScanProfile?.TwainImpl ?? TwainImpl.Default;
            if (UseWorker)
            {
                using(var worker = workerServiceFactory.Create())
                {
                    return worker.Service.TwainGetDeviceList(twainImpl);
                }
            }
            return twainWrapper.GetDeviceList(twainImpl);
        }

        protected override async Task ScanInternal(ScannedImageSource.Concrete source)
        {
            await Task.Factory.StartNew(() =>
            {
                if (UseWorker)
                {
                    RunInForm(formFactory.Create<FTwainGui>(), () =>
                    {
                        using (var worker = workerServiceFactory.Create())
                        {
                            worker.Service.SetRecoveryFolder(RecoveryImage.RecoveryFolder.FullName);
                            worker.Callback.ImageCallback += source.Put;
                            worker.Service.TwainScan(RecoveryImage.RecoveryFileNumber, ScanDevice, ScanProfile, ScanParams, DialogParent.Handle);
                            worker.Callback.WaitForFinish();
                        }
                    });
                }
                else
                {
                    ((FormBase) DialogParent).SafeInvoke(() => twainWrapper.Scan(DialogParent, false, ScanDevice, ScanProfile, ScanParams, source));
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void RunInForm(FormBase form, Action action)
        {
            Exception error = null;
            bool done = false;

            form.Shown += (sender, args) =>
            {
                try
                {
                    form.SafeInvoke(action);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                finally
                {
                    done = true;
                    form.Close();
                }
            };
            form.Closing += (sender, args) =>
            {
                if (!done)
                {
                    args.Cancel = true;
                }
            };
            form.ShowDialog();

            if (error != null)
            {
                if (error is ScanDriverException)
                {
                    throw error;
                }
                throw new ScanDriverUnknownException(error);
            }
        }
    }
}
