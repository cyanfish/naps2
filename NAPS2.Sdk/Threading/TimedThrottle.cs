using System;
using System.Threading;

namespace NAPS2.Threading
{
    public class TimedThrottle
    {
        private readonly Action _action;
        private readonly TimeSpan _interval;
        private Timer _timer;
        private DateTime _lastRun = DateTime.MinValue;

        public TimedThrottle(Action action, TimeSpan interval)
        {
            _action = action;
            _interval = interval;
        }

        public void RunAction(SynchronizationContext syncContext)
        {
            bool doRunAction = false;
            lock (this)
            {
                if (_timer == null && _lastRun < DateTime.Now - _interval)
                {
                    doRunAction = true;
                    _lastRun = DateTime.Now;
                }
                else if (_timer == null)
                {
                    _timer = new Timer(Tick, syncContext, _interval, TimeSpan.FromMilliseconds(-1));
                }
            }

            if (doRunAction)
            {
                _action();
            }
        }

        private void Tick(object state)
        {
            var syncContext = (SynchronizationContext) state;
            lock (this)
            {
                _timer?.Dispose();
                _timer = null;
                _lastRun = DateTime.Now;
            }
            
            syncContext.Post(_ => _action(), null);
        }
    }
}