using System.Windows.Forms;
using NAPS2.WinForms;

namespace NAPS2.Recovery;

internal class RecoveryOperation : OperationBase
{
    private readonly IFormFactory _formFactory;
    private readonly RecoveryManager _recoveryManager;

    public RecoveryOperation(IFormFactory formFactory, RecoveryManager recoveryManager)
    {
        _formFactory = formFactory;
        _recoveryManager = recoveryManager;

        ProgressTitle = MiscResources.ImportProgress;
        AllowCancel = true;
        AllowBackground = true;
    }

    public bool Start(Action<ProcessedImage> imageCallback, RecoveryParams recoveryParams)
    {
        Status = new OperationStatus
        {
            StatusText = MiscResources.Recovering
        };

        var recoverableFolder = _recoveryManager.GetLatestRecoverableFolder();
        if (recoverableFolder == null)
        {
            return false;
        }
        try
        {
            switch (PromptToRecover(recoverableFolder))
            {
                case DialogResult.Yes: // Recover
                    RunAsync(() =>
                    {
                        try
                        {
                            return recoverableFolder.TryRecover(imageCallback, recoveryParams, OnProgress, CancelToken);
                        }
                        finally
                        {
                            recoverableFolder.Dispose();
                            GC.Collect();
                        }
                    });
                    return true;
                case DialogResult.No: // Delete
                    recoverableFolder.TryDelete();
                    recoverableFolder.Dispose();
                    break;
                default: // Not Now
                    recoverableFolder.Dispose();
                    break;
            }
        }
        catch (Exception)
        {
            recoverableFolder.Dispose();
            throw;
        }
        return false;
    }

    private DialogResult PromptToRecover(RecoverableFolder recoverableFolder)
    {
        var recoveryPromptForm = _formFactory.Create<FRecover>();
        recoveryPromptForm.SetData(recoverableFolder.ImageCount, recoverableFolder.ScannedDateTime);
        return recoveryPromptForm.ShowDialog();
    }
}