namespace NAPS2.EtoForms.Mac;

public class CustomFlowLayout : NSCollectionViewFlowLayout
{
    public override NSCollectionViewLayoutAttributes[] GetLayoutAttributesForElements(CGRect rect)
    {
        var layoutAttrs = base.GetLayoutAttributesForElements(rect);
        if (layoutAttrs.Length == 0) return layoutAttrs;

        if (TopAlign)
        {
            UseTopAlign(layoutAttrs);
        }
        if (LeftGravity)
        {
            UseLeftGravity(layoutAttrs);
        }
        return layoutAttrs;
    }

    private void UseTopAlign(NSCollectionViewLayoutAttributes[] layoutAttrs)
    {
        var rows = layoutAttrs.GroupBy(attr => attr.Frame.Y + attr.Frame.Height / 2);
        foreach (var row in rows)
        {
            var minY = row.Min(attr => attr.Frame.Y);
            foreach (var attr in row)
            {
                attr.Frame = attr.Frame with { Y = minY };
            }
        }
    }

    private void UseLeftGravity(NSCollectionViewLayoutAttributes[] layoutAttrs)
    {
        // TODO: This glitches out selection. Fixable?
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
    }

    public bool TopAlign { get; set; }
    public bool LeftGravity { get; set; }
}