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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.WinForms;
using Ninject;
using Ninject.Parameters;
using NLog;

namespace NAPS2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var cultureId = KernelManager.Kernel.Get<UserConfigManager>().Config.Culture;
            if (cultureId != null)
            {
                var culture = new CultureInfo(cultureId);
                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += UnhandledException;

            Application.Run(KernelManager.Kernel.Get<FDesktop>());
        }

        private static void UnhandledException(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
        {
            KernelManager.Kernel.Get<Logger>().FatalException("An error occurred that caused the application to close.", threadExceptionEventArgs.Exception);
        }
    }
}
