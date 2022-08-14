using NAPS2.WinForms;

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
        // TODO: We might want to use an eto-based progress form, or at least show some kind of indicator
        // Where is this method called from anyway?

        // if (!op.IsFinished)
        // {
        //     var form = _formFactory.Create<FProgress>();
        //     form.Operation = op;
        //     form.ShowDialog();
        // }
        op.Wait();
    }

    public override void ShowBackgroundProgress(IOperation op)
    {
    }

    public override List<IOperation> ActiveOperations => throw new NotSupportedException();
}