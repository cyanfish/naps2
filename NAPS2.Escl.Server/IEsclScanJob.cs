namespace NAPS2.Escl.Server;

public interface IEsclScanJob
{
    void Cancel();
    void RegisterStatusChangeCallback(Action<JobStatus> callback);
    Task<bool> WaitForNextDocument();
    void WriteDocumentTo(Stream stream);
    Task WriteProgressTo(Stream stream);
}