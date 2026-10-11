namespace NAPS2.Images;

internal class ImageMetadataUndoElement(
    List<UiImage> images,
    List<ImageMetadata> beforeMetadata,
    List<ImageMetadata> afterMetadata)
    : IUndoElement
{
    public void ApplyUndo() => ReplaceMetadata(beforeMetadata);

    public void ApplyRedo() => ReplaceMetadata(afterMetadata);

    private void ReplaceMetadata(List<ImageMetadata> replaceWith)
    {
        for (int i = 0; i < images.Count; i++)
        {
            if (!images[i].IsDisposed)
            {
                images[i].ReplaceMetadata(replaceWith[i]);
            }
        }
    }

    public static ImageMetadataUndoElement? FromFullList(
        List<UiImage> before, List<ImageMetadata> allBeforeMetadata,
        List<UiImage> after, List<ImageMetadata> allAfterMetadata)
    {
        if (!before.SequenceEqual(after))
        {
            return null;
        }
        var images = new List<UiImage>();
        var beforeMetadata = new List<ImageMetadata>();
        var afterMetadata = new List<ImageMetadata>();
        for (int i = 0; i < before.Count; i++)
        {
            if (allBeforeMetadata[i] != allAfterMetadata[i])
            {
                images.Add(before[i]);
                beforeMetadata.Add(allBeforeMetadata[i]);
                afterMetadata.Add(allAfterMetadata[i]);
            }
        }
        if (images.Count > 0)
        {
            return new ImageMetadataUndoElement(images, beforeMetadata, afterMetadata);
        }
        return null;
    }
}
