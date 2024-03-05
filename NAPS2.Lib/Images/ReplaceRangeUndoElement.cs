namespace NAPS2.Images;

internal class ReplaceRangeUndoElement(
    UiImageList imageList,
    List<UiImage> beforeRange,
    List<UiImage> afterRange)
    : IUndoElement
{
    public void ApplyUndo() => imageList.Mutate(
        new ListMutation<UiImage>.ReplaceRange(afterRange, beforeRange),
        updateUndoStack: false);

    public void ApplyRedo() => imageList.Mutate(
        new ListMutation<UiImage>.ReplaceRange(beforeRange, afterRange),
        updateUndoStack: false);

    public static ReplaceRangeUndoElement? FromFullList(
        UiImageList imageList, List<UiImage> before, List<UiImage> after)
    {
        if (!new HashSet<UiImage>(before).SetEquals(after))
        {
            // Can't undo/redo additions or deletions
            return null;
        }
        int firstDelta = -1;
        int lastDelta = -1;
        for (int i = 0; i < before.Count; i++)
        {
            if (before[i] != after[i])
            {
                if (firstDelta == -1) firstDelta = i;
                lastDelta = i;
            }
        }
        if (firstDelta != -1)
        {
            var beforeRange = before.Skip(firstDelta).Take(lastDelta - firstDelta + 1).ToList();
            var afterRange = after.Skip(firstDelta).Take(lastDelta - firstDelta + 1).ToList();
            return new ReplaceRangeUndoElement(imageList, beforeRange, afterRange);
        }
        return null;
    }
}