using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Images.Storage;

namespace NAPS2.Worker
{
    public class WorkerServiceFactory : IWorkerServiceFactory
    {
        public WorkerContext Create()
        {
            var worker = WorkerManager.NextWorker();
            try
            {
                // TODO: Simplify
                worker.Service.Init(((RecoveryStorageManager)FileStorageManager.Current).RecoveryFolderPath);
            }
            catch (EndpointNotFoundException)
            {
                // Retry once
                worker = WorkerManager.NextWorker();
                worker.Service.Init(((RecoveryStorageManager)FileStorageManager.Current).RecoveryFolderPath);
            }
            return worker;
        }
    }
}
