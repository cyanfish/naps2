namespace NAPS2.Images;

public interface IUndoElement
{
    void ApplyUndo();
    void ApplyRedo();
}