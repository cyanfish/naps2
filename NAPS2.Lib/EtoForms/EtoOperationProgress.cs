using Eto.Forms;
using NAPS2.EtoForms.Ui;

namespace NAPS2.EtoForms;

public class EtoOperationProgress : OperationProgress
{
    private readonly IFormFactory _formFactory;
    private readonly INotificationManager _notificationManager;
    private readonly Naps2Config _config;

    private readonly HashSet<IOperation> _activeOperations = new();

    public EtoOperationProgress(IFormFactory formFactory, INotificationManager notificationManager, Naps2Config config)
    {
        _formFactory = formFactory;
        _notificationManager = notificationManager;
        _config = config;
    }

    public override void Attach(IOperation op)
    {
        lock (this)
        {
            if (!_activeOperations.Contains(op))
            {
                _activeOperations.Add(op);
                op.Finished += (sender, args) => _activeOperations.Remove(op);
                if (op.IsFinished) _activeOperations.Remove(op);
            }
        }
    }

    public override void ShowProgress(IOperation op)
    {
        if (PlatformCompat.System.ShouldRememberBackgroundOperations &&
            _config.Get(c => c.BackgroundOperations).Contains(op.GetType().Name))
        {
            ShowBackgroundProgress(op);
        }
        else
        {
            ShowModalProgress(op);
        }
    }

    public override void ShowModalProgress(IOperation op)
    {
        Attach(op);

        var bgOps = _config.Get(c => c.BackgroundOperations);
        bgOps = bgOps.Remove(op.GetType().Name);
        _config.User.Set(c => c.BackgroundOperations, bgOps);

        if (!op.IsFinished)
        {
            Invoker.Current.Invoke(() =>
            {
                var form = _formFactory.Create<ProgressForm>();
                form.Operation = op;
                form.ShowModal();
            });
        }

        if (!op.IsFinished)
        {
            ShowBackgroundProgress(op);
        }
    }

    public override void ShowBackgroundProgress(IOperation op)
    {
        Attach(op);

        var bgOps = _config.Get(c => c.BackgroundOperations);
        bgOps = bgOps.Add(op.GetType().Name);
        _config.User.Set(c => c.BackgroundOperations, bgOps);

        if (!op.IsFinished)
        {
            Invoker.Current.Invoke(() => _notificationManager.OperationProgress(this, op));
        }
    }

    public static void RenderStatus(IOperation op, Label textLabel, Label numberLabel, ProgressBar progressBar)
    {
        var status = op.Status ?? new OperationStatus();
        textLabel.Text = status.StatusText;
        progressBar.Indeterminate = status.MaxProgress == 1 || status.IndeterminateProgress;
        // TODO: Continuous?
        if (status.MaxProgress == 1 || status.ProgressType == OperationProgressType.None)
        {
            numberLabel.Text = "";
        }
        else if (status.MaxProgress == 0)
        {
            numberLabel.Text = "";
            progressBar.MaxValue = 1;
            progressBar.Value = 0;
        }
        else if (status.ProgressType == OperationProgressType.BarOnly)
        {
            numberLabel.Text = "";
            progressBar.MaxValue = status.MaxProgress;
            progressBar.Value = status.CurrentProgress;
        }
        else
        {
            numberLabel.Text = status.ProgressType == OperationProgressType.MB
                ? string.Format(MiscResources.SizeProgress, (status.CurrentProgress / 1000000.0).ToString("f1"),
                    (status.MaxProgress / 1000000.0).ToString("f1"))
                : string.Format(MiscResources.ProgressFormat, status.CurrentProgress, status.MaxProgress);
            progressBar.MaxValue = status.MaxProgress;
            progressBar.Value = status.CurrentProgress;
        }
        // Force the progress bar to render immediately
        if (progressBar.Value < progressBar.MaxValue)
        {
            progressBar.Value += 1;
            progressBar.Value -= 1;
        }
    }

    public override List<IOperation> ActiveOperations
    {
        get
        {
            lock (_activeOperations)
            {
                return _activeOperations.ToList();
            }
        }
    }
}