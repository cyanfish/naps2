namespace NAPS2.Util;

public class DeferredAction
{
    private readonly Action _action;
    private int _counter;

    public DeferredAction(Action action)
    {
        _action = action;
    }

    public bool IsDeferred => _counter > 0;

    public IDisposable Defer()
    {
        return new DeferSaveObject(this);
    }

    private class DeferSaveObject : IDisposable
    {
        private readonly DeferredAction _deferredAction;

        private bool _disposed;

        public DeferSaveObject(DeferredAction deferredAction)
        {
            _deferredAction = deferredAction;
            lock (deferredAction)
            {
                deferredAction._counter += 1;
            }
        }

        public void Dispose()
        {
            lock (_deferredAction)
            {
                if (_disposed) return;
                _disposed = true;

                _deferredAction._counter -= 1;
                if (!_deferredAction.IsDeferred)
                {
                    _deferredAction._action();
                }
            }
        }
    }
}