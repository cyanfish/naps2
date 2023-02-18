namespace NAPS2.Threading;

/// <summary>
/// Throttles an action by guaranteeing only one invocation can take place at a time. If an invocation is already in
/// progress, another will be queued. Multiple queued invocations are de-duplicated.
///
/// This is useful for typical "refresh" actions where we want to do an expensive rendering whenever the state changes
/// but running it multiple times for the same state is redundant.
/// </summary>
public class RefreshThrottle
{
    private readonly Action _action;
    private bool _isRunningAction;
    private bool _hasQueuedAction;

    public RefreshThrottle(Action action)
    {
        _action = action;
    }

    /// <summary>
    /// Runs the throttled action as a task.
    /// </summary>
    public void RunAction()
    {
        lock (this)
        {
            if (_isRunningAction)
            {
                _hasQueuedAction = true;
                return;
            }
            _isRunningAction = true;
        }
        Task.Run(DoRunAction);
    }

    private void DoRunAction()
    {
        try
        {
            _action();
        }
        finally
        {
            lock (this)
            {
                if (_hasQueuedAction)
                {
                    Task.Run(DoRunAction);
                    _hasQueuedAction = false;
                }
                else
                {
                    _isRunningAction = false;
                }
            }
        }
    }
}