using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms
{
    public static class L
    {
        public static Padding DefaultPadding { get; set; } = new Padding(10);
        public static Size DefaultSpacing { get; set; } = new Size(2, 2);
        
        public static LayoutColumn Column(params LayoutElement[] children) =>
            new LayoutColumn(children);
        
        public static LayoutRow Row(params LayoutElement[] children) =>
            new LayoutRow(children);

        public static DynamicLayout Create(params LayoutElement[] children)
        {
            // TODO: Default padding etc?
            var layout = new DynamicLayout
            {
                DefaultSpacing = DefaultSpacing,
                Padding = DefaultPadding
            };
            foreach (var child in children)
            {
                child.AddTo(layout);
            }
            return layout;
        }
    }
}
