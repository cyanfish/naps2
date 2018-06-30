using NAPS2.DI.Modules;
using NAPS2.Util;
using NAPS2.WinForms;
using Ninject;
using System.Threading;
using System.Windows.Forms;

namespace NAPS2.DI.EntryPoints
{
    public static class WinFormsEntryPoint
    {
        public static void Run(string[] args)
        {
            var kernel = new StandardKernel(new CommonModule(), new WinFormsModule());

            var lifecycle = kernel.Get<Lifecycle>();
            lifecycle.ParseArgs(args);
            lifecycle.ExitIfRedundant();

            kernel.Get<CultureInitializer>().InitCulture(Thread.CurrentThread);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += UnhandledException;

            var formFactory = kernel.Get<IFormFactory>();
            Application.Run(formFactory.Create<FDesktop>());
        }

        private static void UnhandledException(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
        {
            Log.FatalException("An error occurred that caused the application to close.", threadExceptionEventArgs.Exception);
        }
    }
}