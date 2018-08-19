using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using NAPS2.Util;

namespace NAPS2.Worker
{
    public class WorkerCallback : IWorkerCallback
    {
        private bool finished;
        private bool success;
        private Exception exception;

        public bool Progress(int current, int max)
        {
            return OnProgress?.Invoke(current, max) ?? false;
        }

        public event ProgressHandler OnProgress;

        public void Finish(bool success)
        {
            lock (this)
            {
                finished = true;
                this.success = success;
                Monitor.Pulse(this);
            }
        }

        public bool WaitForFinish()
        {
            lock (this)
            {
                if (!finished)
                {
                    Monitor.Wait(this);
                }
                if (exception != null)
                {
                    exception.PreserveStackTrace();
                    throw exception;
                }
                return success;
            }
        }

        public void Error(byte[] serializedException)
        {
            exception = (Exception)new NetDataContractSerializer().Deserialize(new MemoryStream(serializedException));
        }
    }
}