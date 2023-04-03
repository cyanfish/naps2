using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Notifications;

public class ProgressNotification : Notification
{
    private readonly OperationProgress _operationProgress;
    private readonly IOperation _op;

    private readonly Label _textLabel = new();
    private readonly Label _numberLabel = new();
    private readonly ProgressBar _progressBar = new();
    private readonly LayoutVisibility _numberVis = new(true);

    public ProgressNotification(OperationProgress operationProgress, IOperation op)
    {
        _operationProgress = operationProgress;
        _op = op;

        ShowClose = false;
        op.StatusChanged += OnStatusChanged;
        op.Finished += OnFinished;
        if (op.IsFinished)
        {
            Invoker.Current.Invoke(() => Manager?.Hide(this));
        }
        _textLabel.MouseUp += (_, _) => NotificationClicked();
        _numberLabel.MouseUp += (_, _) => NotificationClicked();
        _progressBar.MouseUp += (_, _) => NotificationClicked();
        UpdateStatus();
    }

    private void OnStatusChanged(object? sender, EventArgs eventArgs)
    {
        Invoker.Current.Invoke(UpdateStatus);
    }

    private void OnFinished(object? sender, EventArgs e)
    {
        Invoker.Current.Invoke(() => Manager?.Hide(this));
    }

    private void UpdateStatus()
    {
        EtoOperationProgress.RenderStatus(_op, _textLabel, _numberLabel, _progressBar);
        // Don't display the number if the progress bar is precise
        // Otherwise, the widget will be too cluttered
        // The number is only shown for OcrOperation at the moment
        _numberVis.IsVisible = _op.Status?.IndeterminateProgress == true;
    }

    protected override void NotificationClicked()
    {
        Manager!.Hide(this);
        _operationProgress.ShowModalProgress(_op);
    }

    public override void Dispose()
    {
        base.Dispose();
        _op.StatusChanged -= OnStatusChanged;
        _op.Finished -= OnFinished;
    }

    protected override LayoutElement PrimaryContent => L.Row(
        _textLabel,
        C.Filler().Visible(_numberVis),
        _numberLabel.Visible(_numberVis));

    protected override LayoutElement SecondaryContent => _progressBar.MaxHeight(10);
}