namespace NAPS2.Escl.Server;

internal class FakeEsclScanJob : IEsclScanJob
{
    private Action<StatusTransition>? _callback;

    public void Cancel()
    {
        _callback?.Invoke(StatusTransition.CancelJob);
    }

    public void RegisterStatusTransitionCallback(Action<StatusTransition> callback)
    {
        _callback = callback;
    }

    public Task<bool> WaitForNextDocument() => Task.FromResult(true);

    public void WriteDocumentTo(Stream stream)
    {
        var bytes = File.ReadAllBytes(@"C:\Devel\VS\NAPS2\NAPS2.Sdk.Tests\Resources\dog.jpg");
        stream.Write(bytes, 0, bytes.Length);
        _callback?.Invoke(StatusTransition.DeviceIdle);
    }

    public Task WriteProgressTo(Stream stream) => Task.CompletedTask;
}