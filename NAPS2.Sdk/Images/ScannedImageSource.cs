namespace NAPS2.Images;

// TODO: Add a method or something for IAsyncEnumerable, conditionally compiling for .NET 5+
public abstract class ScannedImageSource
{
    public static ScannedImageSource Empty => new EmptySource();

    public abstract Task<ProcessedImage?> Next();

    public async Task<List<ProcessedImage>> ToList()
    {
        var list = new List<ProcessedImage>();
        try
        {
            await ForEach(image => list.Add(image));
        }
        catch (Exception)
        {
            // TODO: If we ever allow multiple enumeration, this will need to be rethought
            foreach (var image in list)
            {
                image.Dispose();
            }

            throw;
        }

        return list;
    }

    public async Task ForEach(Action<ProcessedImage> action)
    {
        ProcessedImage? image;
        while ((image = await Next()) != null)
        {
            action(image);
        }
    }

    public async Task ForEach(Func<ProcessedImage, Task> action)
    {
        ProcessedImage? image;
        while ((image = await Next()) != null)
        {
            await action(image);
        }
    }

    private class EmptySource : ScannedImageSource
    {
        public override Task<ProcessedImage?> Next() => Task.FromResult<ProcessedImage?>(null);
    }
}