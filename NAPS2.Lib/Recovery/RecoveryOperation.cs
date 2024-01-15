using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;

namespace NAPS2.Recovery;

internal class RecoveryOperation : OperationBase
{
    private readonly IFormFactory _formFactory;
    private readonly RecoveryManager _recoveryManager;

    public RecoveryOperation(IFormFactory formFactory, RecoveryManager recoveryManager)
    {
        _formFactory = formFactory;
        _recoveryManager = recoveryManager;

        ProgressTitle = MiscResources.RecoveryProgress;
        AllowCancel = true;
        AllowBackground = true;
    }

    public bool Start(Action<ProcessedImage> imageCallback, Action<IEnumerable<ProcessedImage>> imageBatchCallback,
        RecoveryParams recoveryParams)
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
            switch (recoveryParams.AutoSessionRestore ? RecoverAction.Recover : PromptToRecover(recoverableFolder))
            {
                case RecoverAction.Recover:
                    Action<Func<bool>> runFunc = recoveryParams.AutoSessionRestore ? RunSync : RunAsync;
                    runFunc(() =>
                    {
                        try
                        {
                            return recoverableFolder.TryRecover(imageCallback, imageBatchCallback, recoveryParams,
                                ProgressHandler);
                        }
                        finally
                        {
                            recoverableFolder.Dispose();
                            GC.Collect();
                        }
                    });
                    return true;
                case RecoverAction.Delete:
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

    private RecoverAction PromptToRecover(RecoverableFolder recoverableFolder)
    {
        var recoveryPromptForm = _formFactory.Create<RecoverForm>();
        recoveryPromptForm.SetData(recoverableFolder.ImageCount, recoverableFolder.ScannedDateTime);
        recoveryPromptForm.ShowModal();
        return recoveryPromptForm.SelectedAction;
    }
}