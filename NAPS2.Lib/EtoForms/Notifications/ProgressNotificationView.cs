using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Notifications;

public class ProgressNotificationView : NotificationView
{
    private readonly OperationProgress _operationProgress;
    private readonly IOperation _op;

    private readonly Label _textLabel = new();
    private readonly Label _numberLabel = new();
    private readonly ProgressBar _progressBar = new();
    private readonly LayoutVisibility _numberVis = new(true);

    public ProgressNotificationView(ProgressNotification model)
        : base(model)
    {
        _operationProgress = model.Progress;
        _op = model.Op;

        ShowClose = false;
        _op.StatusChanged += OnStatusChanged;
        _op.Finished += OnFinished;
        if (_op.IsFinished)
        {
            Manager?.Hide(Model);
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
        Manager?.Hide(Model);
    }

    private void UpdateStatus()
    {
        var text1 = (_textLabel.Text, _numberLabel.Text);
        EtoOperationProgress.RenderStatus(_op, _textLabel, _numberLabel, _progressBar);
        var text2 = (_textLabel.Text, _numberLabel.Text);
        if (text1 != text2)
        {
            // The text width may have changed, so the notification size could change
            Manager?.InvokeUpdated();
        }
        // Don't display the number if the progress bar is precise
        // Otherwise, the widget will be too cluttered
        // The number is only shown for OcrOperation at the moment
        _numberVis.IsVisible = _op.Status?.IndeterminateProgress == true;
    }

    protected override void NotificationClicked()
    {
        Manager!.Hide(Model);
        _operationProgress.ShowModalProgress(_op);
    }

    public override void Dispose()
    {
        base.Dispose();
        _op.StatusChanged -= OnStatusChanged;
        _op.Finished -= OnFinished;
    }

    protected override LayoutElement PrimaryContent => L.Row(
        _textLabel.DynamicWrap(180).MaxWidth(180),
        C.Filler().Visible(_numberVis),
        _numberLabel.Visible(_numberVis));

    protected override LayoutElement SecondaryContent => _progressBar.MaxHeight(10);
}