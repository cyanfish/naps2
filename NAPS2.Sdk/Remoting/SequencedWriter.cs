using System.Threading.Tasks;
using Grpc.Core;

namespace NAPS2.Remoting;

public class SequencedWriter<T>
{
    private readonly IServerStreamWriter<T> _serverStreamWriter;
    private Task _lastWriteTask = Task.CompletedTask;

    public SequencedWriter(IServerStreamWriter<T> serverStreamWriter)
    {
        _serverStreamWriter = serverStreamWriter;
    }

    public void Write(T item)
    {
        lock (this)
        {
            _lastWriteTask = _lastWriteTask.ContinueWith(t => _serverStreamWriter.WriteAsync(item)).Unwrap();
        }
    }

    public Task WaitForCompletion()
    {
        lock (this)
        {
            return _lastWriteTask;
        }
    }
}