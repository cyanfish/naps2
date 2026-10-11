namespace NAPS2.Images;

/// <summary>
/// Combines multiple undo elements into a single undo step. Elements are undone in reverse order and redone in order.
/// </summary>
public class CompoundUndoElement(List<IUndoElement> elements) : IUndoElement
{
    public static IUndoElement Of(List<IUndoElement> elements) =>
        elements.Count == 1 ? elements[0] : new CompoundUndoElement(elements);

    public void ApplyUndo()
    {
        for (int i = elements.Count - 1; i >= 0; i--)
        {
            elements[i].ApplyUndo();
        }
    }

    public void ApplyRedo()
    {
        foreach (var element in elements)
        {
            element.ApplyRedo();
        }
    }
}
