using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan.Wia;
using NAPS2.Util;
using NAPS2.WinForms;
using Ninject.Modules;

namespace NAPS2.DI.Modules
{
    public class WinFormsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPdfPasswordProvider>().To<WinFormsPdfPasswordProvider>();
            Bind<IErrorOutput>().To<MessageBoxErrorOutput>();
            Bind<IOverwritePrompt>().To<WinFormsOverwritePrompt>();
        }
    }
}
