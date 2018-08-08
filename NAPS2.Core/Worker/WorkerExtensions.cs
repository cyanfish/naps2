using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;

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
