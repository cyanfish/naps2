using Eto.Forms;

namespace NAPS2.EtoForms;

public interface IListView<T> : Util.ISelectable<T>
{
    Control Control { get; }

    // TODO: Maybe convert this back to a Size
    int ImageSize { get; set; }

    event EventHandler SelectionChanged;

    event EventHandler ItemClicked;

    event EventHandler<DropEventArgs> Drop;

    bool AllowDrag { get; set; }

    bool AllowDrop { get; set; }

    void SetItems(IEnumerable<T> items);

    void ApplyDiffs(ListViewDiffs<T> diffs);

    void RegenerateImages();
}