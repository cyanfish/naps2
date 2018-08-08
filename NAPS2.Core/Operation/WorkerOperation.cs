using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NAPS2.Recovery;
using NAPS2.Util;
using NAPS2.Worker;

namespace NAPS2.Operation
{
    public abstract class WorkerOperation : OperationBase
    {
        private readonly IWorkerServiceFactory workerServiceFactory;

        protected WorkerOperation(IWorkerServiceFactory workerServiceFactory)
        {
            this.workerServiceFactory = workerServiceFactory;
        }

        protected virtual bool UseWorker => !Environment.Is64BitProcess;

        public ProgressHandler ProgressProxy { get; set; }

        protected bool DoWork(WorkArgs args)
        {
            if (UseWorker)
            {
                using (var worker = workerServiceFactory.Create())
                {
                    worker.Service.SetRecoveryFolder(RecoveryImage.RecoveryFolder.FullName);
                    worker.Callback.OnProgress += OnProgress;
                    worker.Service.DoOperationWork(GetType().FullName, args);
                    return worker.Callback.WaitForFinish();
                }
            }

            return DoWorkInternal(args);
        }

        protected internal abstract bool DoWorkInternal(WorkArgs args);

        protected override bool OnProgress(int current, int max)
        {
            return ProgressProxy?.Invoke(current, max) ?? base.OnProgress(current, max);
        }

        [KnownType("DerivedTypes")]
        [Serializable]
        public class WorkArgs
        {
            // ReSharper disable once UnusedMember.Local
            private static Type[] DerivedTypes()
            {
                return Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(WorkArgs))).ToArray();
            }
        }
    }
}