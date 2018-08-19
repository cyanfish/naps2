using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.WinForms;

namespace NAPS2.Scan.Sane
{
    public class SaneScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "sane";

        private readonly SaneWrapper saneWrapper;
        private readonly IFormFactory formFactory;

        public SaneScanDriver(SaneWrapper saneWrapper, IFormFactory formFactory)
        {
            this.saneWrapper = saneWrapper;
            this.formFactory = formFactory;
        }

        public override string DriverName => DRIVER_NAME;

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
            return saneWrapper.GetDeviceList().ToList();
        }

        protected override IEnumerable<ScannedImage> ScanInternal()
        {
            throw new NotImplementedException();
        }
    }
}
