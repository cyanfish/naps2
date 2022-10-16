using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public static class L
{
    public static LayoutColumn Column(params LayoutElement[] children) =>
        new LayoutColumn(children);
        
    public static LayoutRow Row(params LayoutElement[] children) =>
        new LayoutRow(children);
}