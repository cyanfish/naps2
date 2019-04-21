using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Images.Storage;

namespace NAPS2.Remoting.Worker
{
    public class WorkerServiceFactory : IWorkerServiceFactory
    {
        public WorkerContext Create()
        {
            var rsm = FileStorageManager.Current as RecoveryStorageManager;
            rsm?.EnsureFolderCreated();
            var worker = WorkerManager.NextWorker();
            try
            {
                worker.Service.Init(rsm?.RecoveryFolderPath);
            }
            catch (EndpointNotFoundException)
            {
                // Retry once
                worker = WorkerManager.NextWorker();
                worker.Service.Init(rsm?.RecoveryFolderPath);
            }
            return worker;
        }
    }
}
