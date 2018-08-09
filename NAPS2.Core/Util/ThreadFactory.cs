using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NAPS2.Util
{
    /// <summary>
    /// A simple factory class for threads that provides an easy interface and automatically sets the thread culture.
    /// </summary>
    public class ThreadFactory
    {
        private readonly CultureInitializer cultureInitializer;

        public ThreadFactory(CultureInitializer cultureInitializer)
        {
            this.cultureInitializer = cultureInitializer;
        }

        public Thread CreateThread(Action action)
        {
            // Using CultureInfo.DefaultThreadCurrentCulture would be eaiser, but it's .NET 4.5 only
            var thread = new Thread(new ThreadStart(action));
            cultureInitializer.InitCulture(thread);
            return thread;
        }

        public Thread StartThread(Action action)
        {
            var thread = CreateThread(action);
            thread.Start();
            return thread;
        }
    }
}
