using System.Threading;

namespace NAPS2.Threading;

/// <summary>
/// Throttles an action by only running it once per interval.
///
/// As an example, take a TimedThrottle with an interval of 1 second:
/// - The first call to RunAction executes the action immediately.
/// - If the next call happens after 1 second or more, it also executes immediately.
/// - If the next call happens within 1 second of the last execution, execution will happen on a timer so that it
///   happens 1 second after the last execution.
/// - If execution is already queued on a timer, the extra call is discarded.
///
/// The end result is that the action will run at most once per second, and every call to RunAction is guaranteed to
/// result in the action being executed at some point in the next second. 
/// </summary>
public class TimedThrottle
{
    private readonly Action _action;
    private readonly TimeSpan _interval;
    private Timer? _timer;
    private DateTime _lastRun = DateTime.MinValue;
    private bool _hasQueuedActionOnSyncContext;

    /// <summary>
    /// Creates an instance of TimedThrottle.
    /// </summary>
    /// <param name="action">The action to throttle.</param>
    /// <param name="interval">The minimum interval between calls to the action.</param>
    public TimedThrottle(Action action, TimeSpan interval)
    {
        _action = action;
        _interval = interval;
    }

    /// <summary>
    /// Runs the throttled action on the given synchronization context. 
    /// </summary>
    /// <param name="syncContext">
    /// The synchronization context to asynchronously run the action on. If null, the action will be run synchronously
    /// (if due) or on the thread pool (if delayed).
    /// </param>
    public void RunAction(SynchronizationContext? syncContext)
    {
        bool doRunAction = false;
        lock (this)
        {
            var timeSinceLastRun = DateTime.Now - _lastRun; 
            if (_timer == null && timeSinceLastRun >= _interval)
            {
                doRunAction = true;
                _lastRun = DateTime.Now;
            }
            else if (_timer == null)
            {
                var timeUntilNextRun = _interval - timeSinceLastRun;
                _timer = new Timer(Tick, syncContext, timeUntilNextRun, TimeSpan.FromMilliseconds(-1));
            }
        }

        if (doRunAction)
        {
            RunActionOnSyncContext(syncContext);
        }
    }

    /// <summary>
    /// Runs the throttled action immediately, bypassing any delay. This also resets the timer so calls to RunAction
    /// will be throttled for the next interval period.
    /// </summary>
    /// <param name="syncContext">
    /// The synchronization context to asynchronously run the action on. If null, the action will be run synchronously.
    /// </param>
    public void RunActionNow(SynchronizationContext? syncContext)
    {
        Tick(syncContext);
    }

    private void Tick(object? state)
    {
        lock (this)
        {
            _timer?.Dispose();
            _timer = null;
            _lastRun = DateTime.Now;
        }
        RunActionOnSyncContext((SynchronizationContext?) state);
    }

    private void RunActionOnSyncContext(SynchronizationContext? syncContext)
    {
        if (syncContext == null)
        {
            _action();
            return;
        }
        lock (this)
        {
            if (_hasQueuedActionOnSyncContext) return;
            _hasQueuedActionOnSyncContext = true;
            syncContext.Post(_ =>
            {
                lock (this)
                {
                    _hasQueuedActionOnSyncContext = false;
                }
                _action();
            }, null);
        }
    }
}