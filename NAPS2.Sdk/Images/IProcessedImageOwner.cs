namespace NAPS2.Images;

internal interface IProcessedImageOwner
{
    void Register(IDisposable internalDisposable);
    void Unregister(IDisposable internalDisposable);
}