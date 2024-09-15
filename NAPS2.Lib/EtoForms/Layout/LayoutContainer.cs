namespace NAPS2.EtoForms.Layout;

public abstract class LayoutContainer : LayoutElement
{
    protected LayoutContainer(IEnumerable<LayoutElement> children)
    {
        Children = ExpandChildren(children);
    }

    protected internal List<LayoutElement> Children { get; }

    public override void Materialize(LayoutContext context)
    {
        foreach (var child in Children)
        {
            child.Materialize(context);
        }
    }
}