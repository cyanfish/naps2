using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NAPS2.Threading;

/// <summary>
/// A class to help with parallelization using a pipeline model, wrapping around the TPL Dataflow library.
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
    private const int DEFAULT_MAX_PARALLELISM = 8;

    /// <summary>
    /// Creates a pipeline to process the given input.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static PipelineStep<T> For<T>(IEnumerable<T> input, CancellationToken cancellationToken = default)
    {
        var identityBlock = new TransformBlock<T, T>(x => x);
        return new PipelineStep<T>(identityBlock, cancellationToken, () =>
        {
            foreach (var item in input)
            {
                identityBlock.Post(item);
            }
            identityBlock.Complete();
        });
    }

    public class PipelineStep<T>
    {
        private readonly ISourceBlock<T> _sourceBlock;
        private readonly CancellationToken _cancellationToken;
        private readonly Action _startPipeline;

        internal PipelineStep(ISourceBlock<T> sourceBlock, CancellationToken cancellationToken, Action startPipeline)
        {
            _sourceBlock = sourceBlock;
            _cancellationToken = cancellationToken;
            _startPipeline = startPipeline;
        }
            
        /// <summary>
        /// Adds a new step to the pipeline.
        /// </summary>
        /// <param name="pipelineStepFunc"></param>
        /// <returns></returns>
        public PipelineStep<T2> Step<T2>(Func<T, T2> pipelineStepFunc)
        {
            return Link(new TransformBlock<T, T2>(pipelineStepFunc, ExecutionOptions()));
        }

        /// <summary>
        /// Adds a new step to the pipeline.
        /// </summary>
        /// <param name="pipelineStepFunc"></param>
        /// <returns></returns>
        public PipelineStep<T2> Step<T2>(Func<T, Task<T2>> pipelineStepFunc)
        {
            return Link(new TransformBlock<T, T2>(pipelineStepFunc, ExecutionOptions()));
        }

        /// <summary>
        /// Adds a new step to the pipeline, where multiple items can be processed at once. Note: order is maintained.
        /// </summary>
        /// <param name="pipelineStepFunc"></param>
        /// <param name="maxParallelism"></param>
        /// <returns></returns>
        public PipelineStep<T2> StepParallel<T2>(Func<T, T2> pipelineStepFunc, int maxParallelism = DEFAULT_MAX_PARALLELISM)
        {
            return Link(new TransformBlock<T, T2>(pipelineStepFunc, ExecutionOptions(maxParallelism)));
        }

        /// <summary>
        /// Adds a new step to the pipeline, where multiple items can be processed at once. Note: order is maintained.
        /// </summary>
        /// <param name="pipelineStepFunc"></param>
        /// <param name="maxParallelism"></param>
        /// <returns></returns>
        public PipelineStep<T2> StepParallel<T2>(Func<T, Task<T2>> pipelineStepFunc, int maxParallelism = DEFAULT_MAX_PARALLELISM)
        {
            return Link(new TransformBlock<T, T2>(pipelineStepFunc, ExecutionOptions(maxParallelism)));
        }

        /// <summary>
        /// Adds a new step to the pipeline, where multiple items can be processed at once. Note: order is maintained.
        /// </summary>
        /// <param name="pipelineStepFunc"></param>
        /// <param name="maxParallelism"></param>
        /// <returns></returns>
        public PipelineStep<T2> StepManyParallel<T2>(Func<T, Task<IEnumerable<T2>>> pipelineStepFunc, int maxParallelism = DEFAULT_MAX_PARALLELISM)
        {
            return Link(new TransformManyBlock<T, T2>(pipelineStepFunc, ExecutionOptions(maxParallelism)));
        }

        /// <summary>
        /// Runs the pipeline with the previously defined steps, returning the result. Blocks until the pipeline is finished.
        /// </summary>
        /// <returns></returns>
        public async Task<List<T>> Run()
        {
            var result = new List<T>();
            var actionBlock = new ActionBlock<T>(item => result.Add(item), ExecutionOptions());
            LinkAndStart(actionBlock);
            await actionBlock.Completion;
            return result;
        }

        /// <summary>
        /// Runs the pipeline with the previously defined steps, performing the specified action on each item in the result. Blocks until the pipeline is finished.
        /// </summary>
        /// <param name="pipelineFinishAction"></param>
        public async Task<bool> Run(Action<T> pipelineFinishAction)
        {
            var actionBlock = new ActionBlock<T>(pipelineFinishAction, ExecutionOptions());
            LinkAndStart(actionBlock);
            return await WaitForCompletion(actionBlock);
        }

        /// <summary>
        /// Runs the pipeline with the previously defined steps, performing the specified action on each item in the result. Blocks until the pipeline is finished.
        /// </summary>
        /// <param name="pipelineFinishAction"></param>
        /// <param name="maxParallelism"></param>
        public async Task<bool> RunParallel(Action<T> pipelineFinishAction, int maxParallelism = DEFAULT_MAX_PARALLELISM)
        {
            var actionBlock = new ActionBlock<T>(pipelineFinishAction, ExecutionOptions(maxParallelism));
            LinkAndStart(actionBlock);
            return await WaitForCompletion(actionBlock);
        }

        private ExecutionDataflowBlockOptions ExecutionOptions()
        {
            return new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cancellationToken
            };
        }

        private ExecutionDataflowBlockOptions ExecutionOptions(int maxParallelism)
        {
            return new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cancellationToken,
                MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, maxParallelism)
            };
        }

        private static DataflowLinkOptions LinkOptions()
        {
            return new DataflowLinkOptions
            {
                PropagateCompletion = true
            };
        }

        private PipelineStep<T2> Link<T2>(IPropagatorBlock<T, T2> transformBlock)
        {
            _sourceBlock.LinkTo(transformBlock, LinkOptions());
            return new PipelineStep<T2>(transformBlock, _cancellationToken, _startPipeline);
        }

        private void LinkAndStart(ActionBlock<T> actionBlock)
        {
            _sourceBlock.LinkTo(actionBlock, LinkOptions());
            _startPipeline();
        }

        private static async Task<bool> WaitForCompletion(ActionBlock<T> actionBlock)
        {
            try
            {
                await actionBlock.Completion;
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}