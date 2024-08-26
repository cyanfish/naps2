using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.Dependencies;
using NAPS2.EtoForms.Layout;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class DownloadProgressForm : EtoDialogBase
{
    private readonly Label _totalStatus = new();
    private readonly Label _fileStatus = new();
    private readonly ProgressBar _totalProgressBar = new();
    private readonly ProgressBar _fileProgressBar = new();

    public DownloadProgressForm(ScanningContext scanningContext, Naps2Config config, IIconProvider iconProvider) :
        base(config)
    {
        Title = UiStrings.DownloadProgressFormTitle;
        Icon = iconProvider.GetFormIcon("text_small");

        Controller = new DownloadController(scanningContext);
        Controller.DownloadError += OnDownloadError;
        Controller.DownloadComplete += (_, _) => Close();
        Controller.DownloadProgress += OnDownloadProgress;
    }

    private void OnDownloadError(object? sender, EventArgs e)
    {
        MessageBox.Show(MiscResources.FilesCouldNotBeDownloaded, MiscResources.DownloadError, MessageBoxButtons.OK,
            MessageBoxType.Error);
    }

    public DownloadController Controller { get; }

    protected override void BuildLayout()
    {
        FormStateController.RestoreFormState = false;

        LayoutController.Content = L.Column(
            _totalStatus,
            EtoPlatform.Current.FormatProgressBar(_totalProgressBar),
            EtoPlatform.Current.FormatProgressBar(_fileProgressBar),
            L.Row(
                _fileStatus.Scale()
                    .Align(EtoPlatform.Current.IsWinForms ? LayoutAlignment.Center : LayoutAlignment.Leading),
                C.Button(UiStrings.Cancel, Close)
            )
        );
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Controller.StartDownloadsAsync().AssertNoAwait();
    }

    private void OnDownloadProgress(object? sender, EventArgs e)
    {
        var f = Controller.FilesDownloaded;
        var fTot = Controller.TotalFiles;
        var c = Controller.CurrentFileProgress;
        var cTot = Controller.CurrentFileSize;
        _totalStatus.Text = string.Format(MiscResources.FilesProgressFormat, f, fTot);
        _totalProgressBar.MaxValue = fTot * 1000;
        _totalProgressBar.Value = f * 1000 + (cTot == 0 ? 0 : (int) (c * 1e3 / cTot));
        _fileStatus.Text =
            string.Format(MiscResources.SizeProgress, (c / 1e6).ToString("f1"), (cTot / 1e6).ToString("f1"));
        if (c > 0)
        {
            _fileProgressBar.MaxValue = (int) cTot;
            _fileProgressBar.Value = (int) c;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        Controller.Stop();
    }
}