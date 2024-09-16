using Eto.Drawing;
using Eto.Forms;

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
        bool okFirst;
        if (EtoPlatform.Current.IsWinForms)
        {
            okFirst = true;
        }
        else if (EtoPlatform.Current.IsMac)
        {
            okFirst = false;
        }
        else if (EtoPlatform.Current.IsGtk)
        {
            // TODO: Check other desktop environments than gnome (cancel first) / kde (ok first)
            var desktop = Environment.GetEnvironmentVariable("XDG_SESSION_DESKTOP") ?? "";
            okFirst = desktop.ToLowerInvariant().Contains("kde");
        }
        else
        {
            throw new InvalidOperationException();
        }
        return okFirst
            ? new ExpandLayoutElement(ok, cancel)
            : new ExpandLayoutElement(cancel, ok);
    }

    public static LayoutElement GroupBox(string title, LayoutElement content)
    {
        return EtoPlatform.Current.CreateGroupBox(title, content);
    }

    public static LayoutElement Buffer(LayoutElement element, int left, int top, int right, int bottom)
    {
        return new BufferLayoutElement(element, left, top, right, bottom);
    }

    public static LayoutLeftPanel LeftPanel(LayoutElement left, LayoutElement right)
    {
        return new LayoutLeftPanel(left, right);
    }
}