using System.Collections.Immutable;
using System.Windows.Forms;

namespace NAPS2.WinForms;

public class WinFormsOperationProgress : OperationProgress
{
    private readonly IFormFactory _formFactory;
    private readonly NotificationManager _notificationManager;
    private readonly ScopedConfig _config;

    private readonly HashSet<IOperation> _activeOperations = new HashSet<IOperation>();

    public WinFormsOperationProgress(IFormFactory formFactory, NotificationManager notificationManager, ScopedConfig config)
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
        if (_config.Get(c => c.BackgroundOperations).Contains(op.GetType().Name))
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

        var bgOps = _config.Get(c => c.BackgroundOperations) ?? ImmutableHashSet<string>.Empty;
        bgOps = bgOps.Remove(op.GetType().Name);
        _config.User.Set(c => c.BackgroundOperations = bgOps);

        if (!op.IsFinished)
        {
            var form = _formFactory.Create<FProgress>();
            form.Operation = op;
            form.ShowDialog();
        }

        if (!op.IsFinished)
        {
            ShowBackgroundProgress(op);
        }
    }

    public override void ShowBackgroundProgress(IOperation op)
    {
        Attach(op);

        var bgOps = _config.Get(c => c.BackgroundOperations) ?? ImmutableHashSet<string>.Empty;
        bgOps = bgOps.Add(op.GetType().Name);
        _config.User.Set(c => c.BackgroundOperations = bgOps);

        if (!op.IsFinished)
        {
            _notificationManager.ParentForm.SafeInvoke(() => _notificationManager.OperationProgress(this, op));
        }
    }

    public static void RenderStatus(IOperation op, Label textLabel, Label numberLabel, ProgressBar progressBar)
    {
        var status = op.Status ?? new OperationStatus();
        textLabel.Text = status.StatusText;
        progressBar.Style = status.MaxProgress == 1 || status.IndeterminateProgress
            ? ProgressBarStyle.Marquee
            : ProgressBarStyle.Continuous;
        if (status.MaxProgress == 1 || status.ProgressType == OperationProgressType.None)
        {
            numberLabel.Text = "";
        }
        else if (status.MaxProgress == 0)
        {
            numberLabel.Text = "";
            progressBar.Maximum = 1;
            progressBar.Value = 0;
        }
        else if (status.ProgressType == OperationProgressType.BarOnly)
        {
            numberLabel.Text = "";
            progressBar.Maximum = status.MaxProgress;
            progressBar.Value = status.CurrentProgress;
        }
        else
        {
            numberLabel.Text = status.ProgressType == OperationProgressType.MB
                ? string.Format(MiscResources.SizeProgress, (status.CurrentProgress / 1000000.0).ToString("f1"), (status.MaxProgress / 1000000.0).ToString("f1"))
                : string.Format(MiscResources.ProgressFormat, status.CurrentProgress, status.MaxProgress);
            progressBar.Maximum = status.MaxProgress;
            progressBar.Value = status.CurrentProgress;
        }
        // Force the progress bar to render immediately
        if (progressBar.Value < progressBar.Maximum)
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