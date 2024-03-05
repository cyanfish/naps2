namespace NAPS2.Images;

internal class TransformImagesUndoElement(
    List<UiImage> images,
    List<TransformState> beforeTransforms,
    List<TransformState> afterTransforms)
    : IUndoElement
{
    public void ApplyUndo() => ReplaceTransforms(afterTransforms, beforeTransforms);

    public void ApplyRedo() => ReplaceTransforms(beforeTransforms, afterTransforms);

    private void ReplaceTransforms(List<TransformState> toReplace, List<TransformState> replaceWith)
    {
        for (int i = 0; i < images.Count; i++)
        {
            if (!images[i].IsDisposed)
            {
                images[i].ReplaceTransformState(toReplace[i], replaceWith[i]);
            }
        }
    }

    public static TransformImagesUndoElement? FromFullList(
        List<UiImage> before, List<TransformState> allBeforeTransforms,
        List<UiImage> after, List<TransformState> allAfterTransforms)
    {
        if (!before.SequenceEqual(after))
        {
            return null;
        }
        var images = new List<UiImage>();
        var beforeTransforms = new List<TransformState>();
        var afterTransforms = new List<TransformState>();
        for (int i = 0; i < before.Count; i++)
        {
            if (allBeforeTransforms[i] != allAfterTransforms[i])
            {
                images.Add(before[i]);
                beforeTransforms.Add(allBeforeTransforms[i]);
                afterTransforms.Add(allAfterTransforms[i]);
            }
        }
        if (images.Count > 0)
        {
            return new TransformImagesUndoElement(images, beforeTransforms, afterTransforms);
        }
        return null;
    }
}