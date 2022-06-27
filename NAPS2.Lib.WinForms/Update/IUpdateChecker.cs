namespace NAPS2.Update;

public interface IUpdateChecker
{
    TimeSpan CheckInterval { get; }
    Task<UpdateInfo> CheckForUpdates();
    UpdateOperation StartUpdate(UpdateInfo update);
}