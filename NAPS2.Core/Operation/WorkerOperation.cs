using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NAPS2.Platform;
using NAPS2.Recovery;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;
using NAPS2.Worker;

namespace NAPS2.Operation
{
    /// <summary>
    /// A base class for operations that can proxy part of the operation to a worker process.
    /// </summary>
    public abstract class WorkerOperation : OperationBase
    {
        private readonly IWorkerServiceFactory workerServiceFactory;

        protected WorkerOperation(IWorkerServiceFactory workerServiceFactory)
        {
            this.workerServiceFactory = workerServiceFactory;
        }

        /// <summary>
        /// A value indicating whether DoWork should proxy to a worker process.
        /// </summary>
        protected virtual bool UseWorker => false;

        public ProgressHandler ProgressProxy { get; set; }

        /// <summary>
        /// Calls DoWorkInternal either directly or through a proxy to a worker process, determined by the value of UseWorker.
        /// </summary>
        /// <param name="args">The arguments to be passed to DoWorkInternal.</param>
        /// <returns>A value indicating whether the operation was successful.</returns>
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

        /// <summary>
        /// The base class for arguments passed to DoWork and consumed by DoWorkInternal.
        ///
        /// Subclasses must ensure that they are serializable by DataContractSerializer and will work as intended cross-process.
        /// </summary>
        [KnownType("DerivedTypes")]
        [Serializable]
        public class WorkArgs
        {
            // ReSharper disable once UnusedMember.Local
            private static Type[] DerivedTypes()
            {
                var argTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(WorkArgs)));
                var transformTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(Transform)));
                return argTypes.Concat(transformTypes).ToArray();
            }
        }
    }
}