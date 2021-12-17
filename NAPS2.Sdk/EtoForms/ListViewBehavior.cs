using Eto.Drawing;
using Eto.Forms;
using IDataObject = Eto.Forms.IDataObject;

namespace NAPS2.EtoForms;

public abstract class ListViewBehavior<T>
{
    public bool MultiSelect { get; protected set; }
        
    public bool ShowLabels { get; protected set; }

    public virtual string GetLabel(T item) => throw new NotSupportedException();

    public abstract Image GetImage(T item);

    public virtual void SetDragData(ListSelection<T> selection, IDataObject dataObject) => throw new NotSupportedException();

    public virtual DragEffects GetDropEffect(IDataObject dataObject) => throw new NotSupportedException();
}