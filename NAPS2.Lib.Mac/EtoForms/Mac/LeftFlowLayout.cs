namespace NAPS2.EtoForms.Mac;

public class LeftFlowLayout : NSCollectionViewFlowLayout
{
    public override NSCollectionViewLayoutAttributes[] GetLayoutAttributesForElements(CGRect rect)
    {
        var layoutAttrs = base.GetLayoutAttributesForElements(rect);
        if (layoutAttrs.Length == 0) return layoutAttrs;

        var leftMargin = SectionInset.Left;
        var lastY = layoutAttrs[0].Frame.Y;
        foreach (var attr in layoutAttrs)
        {
            if (attr.Frame.Y > lastY)
            {
                leftMargin = SectionInset.Left;
            }
            attr.Frame = attr.Frame with { X = leftMargin };
            leftMargin += attr.Frame.Width + MinimumInteritemSpacing;
            lastY = attr.Frame.GetMaxY();
        }
        return layoutAttrs;
    }
}