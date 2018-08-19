using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Automation;
using NAPS2.Dependencies;
using NAPS2.ImportExport.Pdf;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;
using Ninject.Modules;

namespace NAPS2.DI.Modules
{
    public class ConsoleModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPdfPasswordProvider>().To<ConsolePdfPasswordProvider>();
            Bind<IErrorOutput>().To<ConsoleErrorOutput>();
            Bind<IOverwritePrompt>().To<ConsoleOverwritePrompt>();
            Bind<IOperationProgress>().To<ConsoleOperationProgress>();
            Bind<IComponentInstallPrompt>().To<ConsoleComponentInstallPrompt>();
            Bind<ThumbnailRenderer>().To<NullThumbnailRenderer>();
        }
    }
}
