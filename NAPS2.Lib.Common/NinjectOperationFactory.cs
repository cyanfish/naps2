using NAPS2.Logging;
using NAPS2.Operation;
using Ninject;

namespace NAPS2;

public class NinjectOperationFactory : IOperationFactory
{
    private readonly IKernel _kernel;
    private readonly ErrorOutput _errorOutput;

    public NinjectOperationFactory(IKernel kernel, ErrorOutput errorOutput)
    {
        _kernel = kernel;
        _errorOutput = errorOutput;
    }

    public T Create<T>() where T : IOperation
    {
        var op = _kernel.Get<T>();
        op.Error += (sender, args) => _errorOutput.DisplayError(args.ErrorMessage, args.Exception);
        return op;
    }
}