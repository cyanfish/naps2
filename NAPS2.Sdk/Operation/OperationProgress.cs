namespace NAPS2.Operation;

/// <summary>
/// A base class for objects capabable of displaying progress for an operation.
/// </summary>
public abstract class OperationProgress
{
    private static OperationProgress _default = new StubOperationProgress();

    public static OperationProgress Default
    {
        get
        {
            TestingContext.NoStaticDefaults();
            return _default;
        }
        set => _default = value ?? throw new ArgumentNullException(nameof(value));
    }

    public abstract void Attach(IOperation op);

    public abstract void ShowProgress(IOperation op);

    public abstract void ShowModalProgress(IOperation op);

    public abstract void ShowBackgroundProgress(IOperation op);

    public abstract List<IOperation> ActiveOperations { get; }
}