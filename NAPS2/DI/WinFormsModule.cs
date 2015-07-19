using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Wia;
using Ninject.Modules;

namespace NAPS2.DI
{
    public class WinFormsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IWiaTransfer>().To<WinFormsWiaTransfer>();
            Bind<IErrorOutput>().To<MessageBoxErrorOutput>();
        }
    }
}
