using NAPS2.EtoForms;

namespace NAPS2.Automation;

public class ConsoleOperationProgress : OperationProgress
{
    private readonly IFormFactory _formFactory;

    public ConsoleOperationProgress(IFormFactory formFactory)
    {
        _formFactory = formFactory;
    }

    public override void Attach(IOperation op)
    {
    }

    public override void ShowProgress(IOperation op)
    {
        op.Wait();
    }

    public override void ShowModalProgress(IOperation op)
    {
        // TODO: We might want to show some kind of indicator
        // Where is this method called from anyway?
        op.Wait();
    }

    public override void ShowBackgroundProgress(IOperation op)
    {
    }

    public override List<IOperation> ActiveOperations => throw new NotSupportedException();
}