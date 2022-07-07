namespace NAPS2.Operation;

/// <summary>
/// A base class for objects capabable of displaying progress for an operation.
/// </summary>
public abstract class OperationProgress
{
    public abstract void Attach(IOperation op);

    public abstract void ShowProgress(IOperation op);

    public abstract void ShowModalProgress(IOperation op);

    public abstract void ShowBackgroundProgress(IOperation op);

    public abstract List<IOperation> ActiveOperations { get; }
}