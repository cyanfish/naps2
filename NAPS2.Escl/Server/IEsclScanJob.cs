namespace NAPS2.Escl.Server;

public interface IEsclScanJob : IDisposable
{
    string ContentType { get; }
    void Cancel();
    void RegisterStatusTransitionCallback(Action<StatusTransition> callback);
    Task<bool> WaitForNextDocument();
    Task WriteDocumentTo(Stream stream);
    Task WriteProgressTo(Stream stream);
    Task WriteErrorDetailsTo(Stream stream);
}