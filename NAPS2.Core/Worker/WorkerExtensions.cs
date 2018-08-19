using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace NAPS2.Worker
{
    public static class WorkerExtensions
    {
        public static void Error(this IWorkerCallback callback, Exception e)
        {
            var stream = new MemoryStream();
            new NetDataContractSerializer().Serialize(stream, e);
            callback.Error(stream.ToArray());
        }
    }
}
