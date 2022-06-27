namespace NAPS2.Update;

public interface IUpdateChecker
{
    Task<UpdateInfo?> CheckForUpdates();
    UpdateOperation StartUpdate(UpdateInfo update);
}