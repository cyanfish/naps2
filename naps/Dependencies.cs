using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ninject.Modules;
using Ninject;
using NAPS.Email;

namespace NAPS
{
    class Dependencies
    {
        public static IKernel Kernel { get; private set; }

        static Dependencies()
        {
            Kernel = new StandardKernel(new DependenciesModule());
        }

        private class DependenciesModule : NinjectModule
        {
            public override void Load()
            {
                Bind<IPdfExporter>().To<PdfSharpExporter>();
                Bind<IEmailer>().To<MAPIEmailer>();
            }
        }
    }
}
