namespace NAPS2.EtoForms.Layout;

public static class L
{
    public static LayoutColumn Column(params LayoutElement[] children) =>
        new LayoutColumn(children);

    public static LayoutRow Row(params LayoutElement[] children) =>
        new LayoutRow(children);

    public static LayoutOverlay Overlay(params LayoutElement[] children) =>
        new LayoutOverlay(children);

    /// <summary>
    /// Displays "Ok" and "Cancel" type buttons in a platform-dependent order.
    /// </summary>
    /// <param name="ok"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public static LayoutElement OkCancel(LayoutElement ok, LayoutElement cancel)
    {
        return EtoPlatform.Current.ButtonOrder == DialogButtonOrder.PrimarySecondaryClose
            ? new ExpandLayoutElement(ok, cancel)
            : new ExpandLayoutElement(cancel, ok);
    }

    public static LayoutElement GroupBox(string title, LayoutElement content,
        GroupBoxHeaderStyle headerStyle = GroupBoxHeaderStyle.Bold)
    {
        return EtoPlatform.Current.CreateGroupBox(title, content, headerStyle);
    }

    public static LayoutElement Buffer(LayoutElement element, int left, int top, int right, int bottom)
    {
        return new BufferLayoutElement(element, left, top, right, bottom);
    }

    public static LayoutSidePanel LeftPanel(LayoutElement left, LayoutElement right)
    {
        return new LayoutSidePanel(left, right, false);
    }

    public static LayoutSidePanel RightPanel(LayoutElement left, LayoutElement right)
    {
        return new LayoutSidePanel(left, right, true);
    }

    public static LayoutAdaptive Adaptive(LayoutElement wide, LayoutElement narrow)
    {
        return new LayoutAdaptive(wide, narrow);
    }

    public static LayoutScrollable Scrollable(LayoutElement content)
    {
        return new LayoutScrollable(content);
    }
}