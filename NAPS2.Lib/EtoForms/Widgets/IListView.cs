using Eto.Forms;

namespace NAPS2.EtoForms.Widgets;

public interface IListView<T> : Util.ISelectable<T> where T : notnull
{
    Control Control { get; }

    ContextMenu? ContextMenu { get; set; }

    // TODO: Maybe convert this back to a Size
    int ImageSize { get; set; }

    event EventHandler SelectionChanged;

    event EventHandler ItemClicked;

    event EventHandler<DropEventArgs> Drop;

    void SetItems(IEnumerable<T> items);

    void ApplyDiffs(ListViewDiffs<T> diffs);

    void RegenerateImages();
}