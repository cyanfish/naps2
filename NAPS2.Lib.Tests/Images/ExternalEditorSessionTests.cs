using NAPS2.Images.Bitwise;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.Images;

public class ExternalEditorSessionTests : ContextualTests
{
    private readonly UiImageList _imageList = new();

    public ExternalEditorSessionTests()
    {
        SetUpFileStorage();
        _imageList.Mutate(new ListMutation<UiImage>.Append(new UiImage(CreateScannedImage())));
        _imageList.Mutate(new ListMutation<UiImage>.Append(new UiImage(CreateScannedImage())));
    }

    public override void Dispose()
    {
        foreach (var image in _imageList.Images)
        {
            image.Dispose();
        }
        base.Dispose();
    }

    [Fact]
    public async Task RepeatedEdits_DirectWrite()
    {
        var page = _imageList.Images[0];
        foreach (var color in TestColors)
        {
            var path = StartSession(page);
            File.WriteAllBytes(path, Encode(color, path));
            await AssertImageColor(page, color);
        }
    }

    [Fact]
    public async Task RepeatedEdits_RenamedWhileStillOpen()
    {
        // Some editors save to a temp file and rename it into place before closing their handle, so the first load
        // attempt hits a sharing violation and needs to be retried after a delay.
        var page = _imageList.Images[0];
        foreach (var color in TestColors)
        {
            var path = StartSession(page);
            var tempPath = Path.Combine(Path.GetDirectoryName(path)!, "~tmp" + Path.GetExtension(path));
            await using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Delete))
            {
                var bytes = Encode(color, path);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
                File.Move(tempPath, path, true);
                await Task.Delay(200);
            }
            await AssertImageColor(page, color);
        }
    }

    [Fact]
    public async Task FileTemporarilyMissing()
    {
        // Two writes queue a throttled reload, which then runs while the file is missing (as with editors that save
        // via delete+rename). That must be retried, not thrown on the watcher/timer thread.
        var page = _imageList.Images[0];
        var path = StartSession(page);
        await RetryIo(() => File.WriteAllBytes(path, Encode(TestColors[0], path)));
        await RetryIo(() => File.WriteAllBytes(path, Encode(TestColors[0], path)));
        await RetryIo(() => File.Delete(path));
        await Task.Delay(1200);
        await RetryIo(() => File.WriteAllBytes(path, Encode(TestColors[1], path)));
        await AssertImageColor(page, TestColors[1]);
    }

    // The session may have the file open for reading at any moment
    private static async Task RetryIo(Action action)
    {
        for (int i = 0; ; i++)
        {
            try
            {
                action();
                return;
            }
            catch (IOException) when (i < 20)
            {
                await Task.Delay(50);
            }
        }
    }

    private static readonly (byte r, byte g, byte b)[] TestColors = [(255, 0, 0), (0, 0, 255), (0, 255, 0)];

    private string StartSession(UiImage page)
    {
        var session = new ExternalEditorSession(ScanningContext, _imageList, page);
        page.EditorSessions.Add(session);
        return session.TempFilePath;
    }

    private byte[] Encode((byte r, byte g, byte b) color, string path)
    {
        using var image = ImageContext.Create(100, 100, ImagePixelFormat.RGB24);
        new FillColorImageOp(color.r, color.g, color.b, 255).Perform(image);
        var stream = new MemoryStream();
        image.Save(stream, ImageContext.GetFileFormatFromExtension(path));
        return stream.ToArray();
    }

    private static async Task AssertImageColor(UiImage page, (byte r, byte g, byte b) expected)
    {
        (int r, int g, int b) actual = default;
        for (int i = 0; i < 50; i++)
        {
            await Task.Delay(100);
            using var processed = page.GetClonedImage();
            using var rendered = processed.Render();
            actual = GetPixel(rendered, 10, 10);
            if (IsClose(actual, expected)) return;
        }
        Assert.Fail($"Expected {expected}, got {actual}");
    }

    private static bool IsClose((int r, int g, int b) a, (byte r, byte g, byte b) b) =>
        Math.Abs(a.r - b.r) <= 5 && Math.Abs(a.g - b.g) <= 5 && Math.Abs(a.b - b.b) <= 5;

    private static unsafe (int r, int g, int b) GetPixel(IMemoryImage image, int x, int y)
    {
        using var lockState = image.Lock(LockMode.ReadOnly, out var data);
        var pixel = data.ptr + data.stride * y + data.bytesPerPixel * x;
        return (*(pixel + data.rOff), *(pixel + data.gOff), *(pixel + data.bOff));
    }
}
