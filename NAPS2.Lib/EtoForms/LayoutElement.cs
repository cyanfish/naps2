using Eto.Forms;

namespace NAPS2.EtoForms;

public abstract class LayoutElement
{
    public abstract void AddTo(DynamicLayout layout);
        
    public static implicit operator LayoutElement(Control control) =>
        new ControlWithLayoutAttributes(control);
        
    public static implicit operator DynamicLayout(LayoutElement element)
    {
        return L.Create(element);
    }
}