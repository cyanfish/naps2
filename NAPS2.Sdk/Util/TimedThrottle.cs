using System;
using System.Threading;

namespace NAPS2.Util
{
    public class TimedThrottle
    {
        private readonly Action action;
        private readonly TimeSpan interval;
        private Timer timer;
        private DateTime lastRun = DateTime.MinValue;

        public TimedThrottle(Action action, TimeSpan interval)
        {
            this.action = action;
            this.interval = interval;
        }

        public void RunAction(SynchronizationContext syncContext)
        {
            bool doRunAction = false;
            lock (this)
            {
                if (timer == null && lastRun < DateTime.Now - interval)
                {
                    doRunAction = true;
                    lastRun = DateTime.Now;
                }
                else if (timer == null)
                {
                    timer = new Timer(Tick, syncContext, interval, TimeSpan.FromMilliseconds(-1));
                }
            }

            if (doRunAction)
            {
                action();
            }
        }

        private void Tick(object state)
        {
            var syncContext = (SynchronizationContext) state;
            lock (this)
            {
                timer?.Dispose();
                timer = null;
                lastRun = DateTime.Now;
            }
            
            syncContext.Post(_ => action(), null);
        }
    }
}