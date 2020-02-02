using System.Threading.Tasks;
using Grpc.Core;

namespace NAPS2.Remoting
{
    public class SequencedWriter<T>
    {
        private readonly IServerStreamWriter<T> serverStreamWriter;
        private Task lastWriteTask = Task.CompletedTask;

        public SequencedWriter(IServerStreamWriter<T> serverStreamWriter)
        {
            this.serverStreamWriter = serverStreamWriter;
        }

        public void Write(T item)
        {
            lock (this)
            {
                lastWriteTask = lastWriteTask.ContinueWith(t => serverStreamWriter.WriteAsync(item)).Unwrap();
            }
        }

        public Task WaitForCompletion()
        {
            lock (this)
            {
                return lastWriteTask;
            }
        }
    }
}