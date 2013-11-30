using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Scan;
using NAPS2.WinForms;
using Ninject;

namespace NAPS2
{
    public class NinjectScanDriverFactory : IScanDriverFactory
    {
        private readonly IKernel kernel;

        public NinjectScanDriverFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IScanDriver Create(string driverName)
        {
            return kernel.Get<IScanDriver>(driverName);
        }
    }
}
