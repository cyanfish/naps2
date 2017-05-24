using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Host;
using NAPS2.Recovery;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.WinForms;

namespace NAPS2.Scan.Twain
{
    public class TwainScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "twain";
        
        private readonly IX86HostServiceFactory x86HostServiceFactory;
        private readonly TwainWrapper twainWrapper;
        private readonly IFormFactory formFactory;

        public TwainScanDriver(IX86HostServiceFactory x86HostServiceFactory, TwainWrapper twainWrapper, IFormFactory formFactory)
        {
            this.x86HostServiceFactory = x86HostServiceFactory;
            this.twainWrapper = twainWrapper;
            this.formFactory = formFactory;
        }

        public override string DriverName => DRIVER_NAME;

        private bool UseHostService => ScanProfile.TwainImpl != TwainImpl.X64 && Environment.Is64BitProcess;

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
            if (UseHostService)
            {
                return x86HostServiceFactory.Create().TwainGetDeviceList(twainImpl);
            }
            return twainWrapper.GetDeviceList(twainImpl);
        }

        protected override IEnumerable<ScannedImage> ScanInternal()
        {
            if (UseHostService)
            {
                return RunInForm(formFactory.Create<FTwainGui>(), () =>
                {
                    var service = x86HostServiceFactory.Create();
                    service.SetRecoveryFolder(RecoveryImage.RecoveryFolder.FullName);
                    return service.TwainScan(RecoveryImage.RecoveryFileNumber, ScanDevice, ScanProfile, ScanParams)
                        .Select(x => new ScannedImage(x))
                        .ToList();
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
