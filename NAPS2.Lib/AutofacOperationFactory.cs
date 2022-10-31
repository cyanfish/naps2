using Autofac;

namespace NAPS2;

public class AutofacOperationFactory : IOperationFactory
{
    private readonly IComponentContext _container;
    private readonly ErrorOutput _errorOutput;

    public AutofacOperationFactory(IComponentContext container, ErrorOutput errorOutput)
    {
        _container = container;
        _errorOutput = errorOutput;
    }

    public T Create<T>() where T : IOperation
    {
        var op = _container.Resolve<T>();
        op.Error += (sender, args) => _errorOutput.DisplayError(args.ErrorMessage, args.Exception);
        return op;
    }
}