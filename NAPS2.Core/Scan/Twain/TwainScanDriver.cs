using System;
using System.Collections.Generic;
using System.Linq;
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

        // 64 bit TWAIN support via worker is experimental.
        // Issue list:
        // - Hard to give focus to the TWAIN UI consistently. Maybe leverage the Form.Activated event in NAPS2.exe to call a new method in NAPS2.Worker.
        // - Relatedly, there's no way to find the TWAIN window from the taskbar. But if the above can work then maybe not needed.
        // - Minor lag (1-2s) when doing the first WCF call. Should be fixable with pre-cached workers.
        // - General stability needs testing/work
        // - Probably something else I forgot. Thorough testing should reveal more issues.
        private bool UseWorker => ScanProfile.TwainImpl == TwainImpl.X64 && !Environment.Is64BitProcess;

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
            return GetFullDeviceList().Where(x => !x.ID.StartsWith("WIA-")).ToList();
        }

        private IEnumerable<ScanDevice> GetFullDeviceList()
        {
            var twainImpl = ScanProfile != null ? ScanProfile.TwainImpl : TwainImpl.Default;
            if (UseWorker)
            {
                using(var service = workerServiceFactory.Create())
                {
                    return service.TwainGetDeviceList(twainImpl);
                }
            }
            return twainWrapper.GetDeviceList(twainImpl);
        }

        protected override IEnumerable<ScannedImage> ScanInternal()
        {
            if (UseWorker)
            {
                return RunInForm(formFactory.Create<FTwainGui>(), () =>
                {
                    using (var service = workerServiceFactory.Create())
                    {
                        service.SetRecoveryFolder(RecoveryImage.RecoveryFolder.FullName);
                        return service.TwainScan(RecoveryImage.RecoveryFileNumber, ScanDevice, ScanProfile, ScanParams)
                            .Select(x => new ScannedImage(x))
                            .ToList();
                    }
                });
            }
            return twainWrapper.Scan(DialogParent, false, ScanDevice, ScanProfile, ScanParams);
        }

        private T RunInForm<T>(FormBase form, Func<T> func) where T : class
        {
            T result = null;
            Exception error = null;
            bool done = false;

            form.Shown += (sender, args) =>
            {
                try
                {
                    result = func();
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

            return result;
        }
    }
}
