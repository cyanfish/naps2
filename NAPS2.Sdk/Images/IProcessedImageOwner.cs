namespace NAPS2.Images;

public interface IProcessedImageOwner
{
    void Register(IDisposable internalDisposable);
    void Unregister(IDisposable internalDisposable);
}