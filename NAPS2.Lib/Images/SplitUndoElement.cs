namespace NAPS2.Images;

public class SplitUndoElement(
    UiImageList imageList,
    UiImage image1,
    UiImage image2,
    TransformState oldTransforms,
    CropTransform transform1,
    CropTransform transform2)
    : IUndoElement
{
    public void ApplyUndo()
    {
        if (imageList.Images.Contains(image1) && imageList.Images.Contains(image2) &&
            image1.TransformState == oldTransforms.AddOrSimplify(transform1) &&
            image2.TransformState == oldTransforms.AddOrSimplify(transform2))
        {
            image1.ReplaceTransformState(image1.TransformState, oldTransforms);
            image2.ReplaceTransformState(image2.TransformState, oldTransforms);
            if (imageList.Selection.Contains(image1))
            {
                imageList.AddToSelection(image2);
            }
            imageList.Mutate(new ListMutation<UiImage>.DeleteSelected(), ListSelection.Of(image1),
                updateUndoStack: false, disposeDeleted: false);
            image1.GetImageWeakReference().ProcessedImage.Dispose();
        }
    }

    public void ApplyRedo()
    {
        if (imageList.Images.Contains(image2) && !imageList.Images.Contains(image1) &&
            image2.TransformState == oldTransforms && !image1.IsDisposed)
        {
            image1.ReplaceInternalImage(image2.GetClonedImage());
            image1.AddTransform(transform1);
            image2.AddTransform(transform2);
            imageList.Mutate(new ListMutation<UiImage>.InsertBefore(image1, image2), ListSelection.Empty<UiImage>(),
                updateUndoStack: false);
            if (imageList.Selection.Contains(image2))
            {
                imageList.AddToSelection(image1);
            }
        }
    }
}