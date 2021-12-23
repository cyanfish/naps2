namespace NAPS2.Images;

public abstract class ScannedImageSource
{
    public static ScannedImageSource Empty => new EmptySource();

    public abstract Task<RenderableImage?> Next();

    public async Task<List<RenderableImage>> ToList()
    {
        var list = new List<RenderableImage>();
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

    public async Task ForEach(Action<RenderableImage> action)
    {
        RenderableImage? image;
        while ((image = await Next()) != null)
        {
            action(image);
        }
    }

    public async Task ForEach(Func<RenderableImage, Task> action)
    {
        RenderableImage? image;
        while ((image = await Next()) != null)
        {
            await action(image);
        }
    }

    private class EmptySource : ScannedImageSource
    {
        public override Task<RenderableImage?> Next() => Task.FromResult<RenderableImage?>(null);
    }
}