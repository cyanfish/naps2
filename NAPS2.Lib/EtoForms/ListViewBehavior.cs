using Eto.Drawing;
using Eto.Forms;
using IDataObject = Eto.Forms.IDataObject;

namespace NAPS2.EtoForms;

public abstract class ListViewBehavior<T> where T : notnull
{
    public bool MultiSelect { get; protected set; }
        
    public bool ShowLabels { get; protected set; }

    public bool ScrollOnDrag { get; protected set; }

    public bool UseHandCursor { get; protected set; }

    public bool Checkboxes { get; protected set; }

    public virtual string GetLabel(T item) => throw new NotSupportedException();

    public virtual Image GetImage(T item, int imageSize) => throw new NotSupportedException();

    public virtual void SetDragData(ListSelection<T> selection, IDataObject dataObject) => throw new NotSupportedException();

    public virtual DragEffects GetDropEffect(IDataObject dataObject) => throw new NotSupportedException();
}