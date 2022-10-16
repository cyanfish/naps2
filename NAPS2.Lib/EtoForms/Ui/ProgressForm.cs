using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public class ProgressForm : EtoDialogBase
{
    private readonly Label _status = new();
    private readonly Label _numeric = new();
    private readonly ProgressBar _progressBar = new();
    private readonly Button _cancel = new() { Text = UiStrings.Cancel };
    private readonly Button _runInBg = new() { Text = UiStrings.RunInBackground };

    private volatile bool _loaded;
    private volatile bool _background;
    private IOperation _operation = null!;

    public ProgressForm(Naps2Config config) : base(config)
    {
        FormStateController.RestoreFormState = false;

        _cancel.Click += Cancel_Click;
        _runInBg.Click += RunInBg_Click;

        Size = new Size();

        LayoutController.Content = L.Column(
            _status,
            _progressBar.Size(420, 40),
            L.Row(
                _numeric,
                C.ZeroSpace().XScale(),
                _runInBg,
                _cancel
            )
        );
    }

    public IOperation Operation
    {
        get => _operation;
        set
        {
            _operation = value;
            _operation.StatusChanged += operation_StatusChanged;
            _operation.Finished += operation_Finished;
            _cancel.Visible = _operation.AllowCancel;
        }
    }

    void operation_StatusChanged(object sender, EventArgs e)
    {
        if (_loaded && !_background)
        {
            Invoker.Current.SafeInvoke(DisplayProgress);
        }
    }

    void operation_Finished(object sender, EventArgs e)
    {
        if (_loaded && !_background)
        {
            Invoker.Current.SafeInvoke(Close);
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _loaded = true;
        Title = _operation.ProgressTitle;
        _runInBg.Visible = _operation.AllowBackground;

        DisplayProgress();
        if (_operation.IsFinished)
        {
            Close();
        }
    }

    private void DisplayProgress()
    {
        EtoOperationProgress.RenderStatus(Operation, _status, _numeric, _progressBar);
    }

    private void Cancel_Click(object sender, EventArgs e)
    {
        TryCancelOp();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (!_operation.IsFinished && !_background)
        {
            TryCancelOp();
            e.Cancel = true;
        }
    }

    private void TryCancelOp()
    {
        if (Operation.AllowCancel)
        {
            Operation.Cancel();
            _cancel.Enabled = false;
        }
    }

    private void RunInBg_Click(object sender, EventArgs e)
    {
        _background = true;
        Close();
    }
}