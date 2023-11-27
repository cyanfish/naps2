namespace NAPS2.Escl.Server;

internal class FakeEsclScanJob : IEsclScanJob
{
    private Action<JobStatus>? _callback;

    public void Cancel()
    {
        _callback?.Invoke(JobStatus.Canceled);
    }

    public void RegisterStatusChangeCallback(Action<JobStatus> callback)
    {
        _callback = callback;
    }

    public Task<bool> WaitForNextDocument() => Task.FromResult(true);

    public void WriteDocumentTo(Stream stream)
    {
        var bytes = File.ReadAllBytes(@"C:\Devel\VS\NAPS2\NAPS2.Sdk.Tests\Resources\dog.jpg");
        stream.Write(bytes, 0, bytes.Length);
        _callback?.Invoke(JobStatus.Completed);
    }

    public Task WriteProgressTo(Stream stream) => Task.CompletedTask;
}