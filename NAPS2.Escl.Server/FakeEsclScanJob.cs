namespace NAPS2.Escl.Server;

internal class FakeEsclScanJob : IEsclScanJob
{
    public void Cancel()
    {
    }

    public Task<bool> WaitForNextDocument() => Task.FromResult(true);

    public void WriteDocumentTo(Stream stream)
    {
        var bytes = File.ReadAllBytes(@"C:\Devel\VS\NAPS2\NAPS2.Sdk.Tests\Resources\dog.jpg");
        stream.Write(bytes, 0, bytes.Length);
    }

    public Task WriteProgressTo(Stream stream) => Task.CompletedTask;
}