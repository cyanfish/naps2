using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NAPS2.Util
{
    /// <summary>
    /// A class to help with parallelization using a pipeline model.
    ///
    /// Pipelines consist of input, a number of steps, and output.
    ///
    /// Pipelines are described using fluent syntax.
    /// <example>
    ///     Pipeline.For(images)
    ///             .Step(LoadImage)
    ///             .Step(RunOcr)
    ///             .Run(OutputText);
    /// </example>
    /// </summary>
    public static class Pipeline
    {
        private static readonly TaskFactory TaskFactory =
            new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

        /// <summary>
        /// Creates a pipeline to process the given input.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IPipelineSyntax<T> For<T>(IEnumerable<T> input)
        {
            return new PipelineSource<T>(input);
        }

        private abstract class PipelineBase<T> : IPipelineSyntax<T>
        {
            public IPipelineSyntax<T2> Step<T2>(Func<T, T2> pipelineStepFunc)
            {
                return new PipelineStep<T, T2>(this, pipelineStepFunc);
            }

            public IPipelineSyntax<T2> StepParallel<T2>(Func<T, T2> pipelineStepFunc)
            {
                return new OrderMaintainingPipelineParallelStep<T, T2>(this, pipelineStepFunc);
            }

            public IPipelineSyntax<T> StepBlock()
            {
                return new PipelineBlockingStep<T>(this);
            }

            public List<T> Run()
            {
                var taskList = new List<Task>();
                var result = GetOutput(taskList);
                Task.WaitAll(taskList.ToArray());
                return result.ToList();
            }

            public void Run(Action<T> pipelineFinishAction)
            {
                var taskList = new List<Task>();
                foreach (var item in GetOutput(taskList))
                {
                    pipelineFinishAction(item);
                }
                Task.WaitAll(taskList.ToArray());
            }

            public abstract IEnumerable<T> GetOutput(List<Task> taskList);
        }

        private class PipelineSource<T> : PipelineBase<T>
        {
            private readonly IEnumerable<T> value;

            public PipelineSource(IEnumerable<T> input)
            {
                value = input;
            }

            public override IEnumerable<T> GetOutput(List<Task> taskList) => value;
        }

        private class PipelineStep<T1, T2> : PipelineBase<T2>
        {
            private readonly PipelineBase<T1> previous;
            private readonly Func<T1, T2> func;

            public PipelineStep(PipelineBase<T1> previous, Func<T1, T2> func)
            {
                this.previous = previous;
                this.func = func;
            }

            public override IEnumerable<T2> GetOutput(List<Task> taskList)
            {
                var collection = new BlockingCollection<T2>();
                var input = previous.GetOutput(taskList);
                taskList.Add(TaskFactory.StartNew(() =>
                {
                    try
                    {
                        foreach (var item in input)
                        {
                            var result = func(item);
                            if (!ReferenceEquals(result, null))
                            {
                                collection.Add(result);
                            }
                        }
                    }
                    finally
                    {
                        collection.CompleteAdding();
                    }
                }));
                return collection.GetConsumingEnumerable();
            }
        }

        private class PipelineBlockingStep<T> : PipelineBase<T>
        {
            private readonly PipelineBase<T> previous;

            public PipelineBlockingStep(PipelineBase<T> previous)
            {
                this.previous = previous;
            }

            public override IEnumerable<T> GetOutput(List<Task> taskList)
            {
                return previous.GetOutput(taskList).ToList();
            }
        }

        private class PipelineParallelStep<T1, T2> : PipelineBase<T2>
        {
            private readonly PipelineBase<T1> previous;
            private readonly Func<T1, T2> func;

            public PipelineParallelStep(PipelineBase<T1> previous, Func<T1, T2> func)
            {
                this.previous = previous;
                this.func = func;
            }

            public override IEnumerable<T2> GetOutput(List<Task> taskList)
            {
                var collection = new BlockingCollection<T2>();
                var input = previous.GetOutput(taskList);
                taskList.Add(TaskFactory.StartNew(() =>
                {
                    try
                    {
                        Parallel.ForEach(input, item =>
                        {
                            var result = func(item);
                            if (!ReferenceEquals(result, null))
                            {
                                collection.Add(result);
                            }
                        });
                    }
                    finally
                    {
                        collection.CompleteAdding();
                    }
                }));
                return collection.GetConsumingEnumerable();
            }
        }

        private class OrderMaintainingPipelineParallelStep<T1, T2> : PipelineBase<T2>
        {
            private readonly PipelineBase<T1> previous;
            private readonly Func<T1, T2> func;

            public OrderMaintainingPipelineParallelStep(PipelineBase<T1> previous, Func<T1, T2> func)
            {
                this.previous = previous;
                this.func = func;
            }

            public override IEnumerable<T2> GetOutput(List<Task> taskList)
            {
                var collection = new BlockingCollection<T2>();
                var input = previous.GetOutput(taskList);
                taskList.Add(TaskFactory.StartNew(() =>
                {
                    try
                    {
                        // Store results in an intermediate container so that we can send them to the actual output in the correct order
                        var resultBuffer = new Dictionary<int, T2>();
                        int outputIndex = 0;
                        Parallel.ForEach(WithIndexAsKey(input), item =>
                        {
                            var result = func(item.Value);
                            lock (resultBuffer)
                            {
                                resultBuffer.Add(item.Key, result);
                                // Check if we have anything ready to send to the actual output
                                // If the "next" result is missing, nothing will be sent yet, and results will just be buffered
                                // After all items are processed, this is guaranteed to send all buffered results
                                while (resultBuffer.ContainsKey(outputIndex))
                                {
                                    var output = resultBuffer[outputIndex];
                                    if (!ReferenceEquals(output, null))
                                    {
                                        collection.Add(output);
                                    }
                                    outputIndex++;
                                }
                            }
                        });
                    }
                    finally
                    {
                        collection.CompleteAdding();
                    }
                }));
                return collection.GetConsumingEnumerable();
            }

            private IEnumerable<KeyValuePair<int, T1>> WithIndexAsKey(IEnumerable<T1> input)
            {
                return input.Zip(WholeNumbers(), (value, key) => new KeyValuePair<int, T1>(key, value));
            }

            private IEnumerable<int> WholeNumbers()
            {
                for (int i = 0; ; i++)
                {
                    yield return i;
                }
            }
        }

        public interface IPipelineSyntax<T>
        {
            /// <summary>
            /// Adds a new step to the pipeline.
            /// </summary>
            /// <param name="pipelineStepFunc"></param>
            /// <returns></returns>
            IPipelineSyntax<T2> Step<T2>(Func<T, T2> pipelineStepFunc);

            /// <summary>
            /// Adds a new step to the pipeline, where multiple items can be processed at once. Note: order is maintained.
            /// </summary>
            /// <param name="pipelineStepFunc"></param>
            /// <returns></returns>
            IPipelineSyntax<T2> StepParallel<T2>(Func<T, T2> pipelineStepFunc);

            /// <summary>
            /// Runs the pipeline with the previously defined steps, returning the result. Blocks until the pipeline is finished.
            /// </summary>
            /// <returns></returns>
            List<T> Run();

            /// <summary>
            /// Runs the pipeline with the previously defined steps, performing the specified action on each item in the result. Blocks until the pipeline is finished.
            /// </summary>
            /// <param name="pipelineFinishAction"></param>
            void Run(Action<T> pipelineFinishAction);

            /// <summary>
            /// Indicates that the pipeline should be blocked until all previous steps are completed before continuing with subsequent steps.
            /// </summary>
            IPipelineSyntax<T> StepBlock();
        }

        #region Extensions for Tuples

        /// <summary>
        /// Adds a new step to the pipeline.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="pipelineStepFunc"></param>
        /// <returns></returns>
        public static IPipelineSyntax<T2> Step<TIn1, TIn2, T2>(this IPipelineSyntax<Tuple<TIn1, TIn2>> syntax, Func<TIn1, TIn2, T2> pipelineStepFunc)
        {
            return syntax.Step(tuple => pipelineStepFunc(tuple.Item1, tuple.Item2));
        }

        /// <summary>
        /// Adds a new step to the pipeline.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="pipelineStepFunc"></param>
        /// <returns></returns>
        public static IPipelineSyntax<T2> Step<TIn1, TIn2, TIn3, T2>(this IPipelineSyntax<Tuple<TIn1, TIn2, TIn3>> syntax, Func<TIn1, TIn2, TIn3, T2> pipelineStepFunc)
        {
            return syntax.Step(tuple => pipelineStepFunc(tuple.Item1, tuple.Item2, tuple.Item3));
        }

        /// <summary>
        /// Adds a new step to the pipeline, where multiple items can be processed at once. Note: order is maintained.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="pipelineStepFunc"></param>
        /// <returns></returns>
        public static IPipelineSyntax<T2> StepParallel<TIn1, TIn2, T2>(this IPipelineSyntax<Tuple<TIn1, TIn2>> syntax, Func<TIn1, TIn2, T2> pipelineStepFunc)
        {
            return syntax.StepParallel(tuple => pipelineStepFunc(tuple.Item1, tuple.Item2));
        }

        /// <summary>
        /// Adds a new step to the pipeline, where multiple items can be processed at once. Note: order is maintained.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="pipelineStepFunc"></param>
        /// <returns></returns>
        public static IPipelineSyntax<T2> StepParallel<TIn1, TIn2, TIn3, T2>(this IPipelineSyntax<Tuple<TIn1, TIn2, TIn3>> syntax, Func<TIn1, TIn2, TIn3, T2> pipelineStepFunc)
        {
            return syntax.StepParallel(tuple => pipelineStepFunc(tuple.Item1, tuple.Item2, tuple.Item3));
        }

        /// <summary>
        /// Runs the pipeline with the previously defined steps, performing the specified action on each item in the result. Blocks until the pipeline is finished.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="pipelineFinishAction"></param>
        public static void Run<TIn1, TIn2>(this IPipelineSyntax<Tuple<TIn1, TIn2>> syntax, Action<TIn1, TIn2> pipelineFinishAction)
        {
            syntax.Run(tuple => pipelineFinishAction(tuple.Item1, tuple.Item2));
        }

        /// <summary>
        /// Runs the pipeline with the previously defined steps, performing the specified action on each item in the result. Blocks until the pipeline is finished.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="pipelineFinishAction"></param>
        public static void Run<TIn1, TIn2, TIn3>(this IPipelineSyntax<Tuple<TIn1, TIn2, TIn3>> syntax, Action<TIn1, TIn2, TIn3> pipelineFinishAction)
        {
            syntax.Run(tuple => pipelineFinishAction(tuple.Item1, tuple.Item2, tuple.Item3));
        }

        #endregion
    }
}
