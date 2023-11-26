namespace NAPS2.Escl.Server;

public interface IEsclScanJob
{
    void Cancel();
    Task<bool> WaitForNextDocument();
    void WriteDocumentTo(Stream stream);
    Task WriteProgressTo(Stream stream);
}