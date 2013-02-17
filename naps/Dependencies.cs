/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009        Pavel Sorejs
    Copyright (C) 2012, 2013  Ben Olden-Cooligan

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
using System.Linq;
using System.Text;

using Ninject.Modules;
using Ninject;

using NAPS.Email;
using NAPS.Pdf;
using NAPS.Scan.Driver;

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
                Bind<IScanDriverFactory>().To<DefaultScanDriverFactory>();
            }
        }
    }
}
