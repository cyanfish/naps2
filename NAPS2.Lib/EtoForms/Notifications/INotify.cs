using NAPS2.Update;

namespace NAPS2.EtoForms.Notifications;

public interface INotify : ISaveNotify
{
    void DonatePrompt();
    void ReviewPrompt();
    void OperationProgress(OperationProgress progress, IOperation op);
    void UpdateAvailable(IUpdateChecker updateChecker, UpdateInfo update);
}