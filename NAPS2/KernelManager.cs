/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Email;
using NAPS2.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Stub;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NAPS2
{
    static class KernelManager
    {
        static KernelManager()
        {
            Kernel = new StandardKernel(new DependenciesModule());
        }

        public static IKernel Kernel { get; private set; }

        private class DependenciesModule : NinjectModule
        {
            public override void Load()
            {
                Bind<IScanPerformer>().To<ScanPerformer>();
                Bind<IProfileManager>().To<ProfileManager>().InSingletonScope();
                Bind<IPdfExporter>().To<PdfSharpExporter>();
                Bind<IEmailer>().To<MAPIEmailer>();
                Bind<IErrorOutput>().To<MessageBoxErrorOutput>();
                Bind<Logger>().ToMethod(LoggerFactory.GetLogger).InSingletonScope();
#if DEBUG && false
                Bind<IScanDriver>().To<StubWiaScanDriver>().Named(WiaScanDriver.DRIVER_NAME);
                Bind<IScanDriver>().To<StubTwainScanDriver>().Named(TwainScanDriver.DRIVER_NAME);
#else
                Bind<IScanDriver>().To<WiaScanDriver>().Named(WiaScanDriver.DRIVER_NAME);
                Bind<IScanDriver>().To<TwainScanDriver>().Named(TwainScanDriver.DRIVER_NAME);
#endif
            }
        }
    }
}
