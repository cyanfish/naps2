using System;
using System.Collections.Generic;
using System.Linq;
using Ninject.Modules;

namespace NAPS2.DI
{
    public class WinFormsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IErrorOutput>().To<MessageBoxErrorOutput>();
        }
    }
}
