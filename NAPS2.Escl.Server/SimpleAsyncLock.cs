namespace NAPS2.Escl.Server;

internal class SimpleAsyncLock
{
    private readonly Queue<TaskCompletionSource<bool>> _listeners = new();
    private bool _isTaken;

    public Task Take()
    {
        lock (this)
        {
            if (!_isTaken)
            {
                _isTaken = true;
                return Task.CompletedTask;
            }
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _listeners.Enqueue(tcs);
            return tcs.Task;
        }
    }

    public void Release()
    {
        lock (this)
        {
            if (_listeners.Count > 0)
            {
                _listeners.Dequeue().SetResult(true);
            }
            else
            {
                _isTaken = false;
            }
        }
    }
}