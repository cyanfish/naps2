using NAPS.Email;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Ninject;

namespace NAPS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(Dependencies.Kernel.Get<FDesktop>());
        }
    }
}