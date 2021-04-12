using System;
using Eto.Drawing;
using Eto.Forms;
using Eto.WinForms;

namespace NAPS2.EtoForms
{
    public static class EtoExtensions
    {
        public static DynamicRow AddSeparateRow(this DynamicLayout layout, params ControlWithLayoutAttributes[] controls)
        {
            layout.BeginVertical();
            var row = layout.BeginHorizontal();
            layout.AddAll(controls);
            layout.EndHorizontal();
            layout.EndVertical();
            return row;
        }
        
        public static void AddAll(this DynamicLayout layout, params ControlWithLayoutAttributes[] controls)
        {
            foreach (var control in controls)
            {
                layout.Add(control);
            }
        }

        public static void Add(this DynamicLayout layout, ControlWithLayoutAttributes control) =>
            control.AddTo(layout);

        public static Icon ToEtoIcon(this System.Drawing.Bitmap bitmap) => new Icon(1f, bitmap.ToEto());

        public static ControlWithLayoutAttributes Center(this Control control) =>
            new ControlWithLayoutAttributes(control, center: true);
        public static ControlWithLayoutAttributes XScale(this Control control) =>
            new ControlWithLayoutAttributes(control, xScale: true);
        public static ControlWithLayoutAttributes YScale(this Control control) =>
            new ControlWithLayoutAttributes(control, yScale: true);
        public static ControlWithLayoutAttributes AutoSize(this Control control) =>
            new ControlWithLayoutAttributes(control, autoSize: true);
        
        public static ControlWithLayoutAttributes Center(this ControlWithLayoutAttributes control) =>
            new ControlWithLayoutAttributes(control, center: true);
        public static ControlWithLayoutAttributes XScale(this ControlWithLayoutAttributes control) =>
            new ControlWithLayoutAttributes(control, xScale: true);
        public static ControlWithLayoutAttributes YScale(this ControlWithLayoutAttributes control) =>
            new ControlWithLayoutAttributes(control, yScale: true);
        public static ControlWithLayoutAttributes AutoSize(this ControlWithLayoutAttributes control) =>
            new ControlWithLayoutAttributes(control, autoSize: true);
        
        public static LayoutColumn Padding(this LayoutColumn column, Padding padding) =>
            new LayoutColumn(column, padding: padding);
        public static LayoutColumn Spacing(this LayoutColumn column, Size spacing) =>
            new LayoutColumn(column, spacing: spacing);
        public static LayoutColumn Spacing(this LayoutColumn column, int xSpacing, int ySpacing) =>
            new LayoutColumn(column, spacing: new Size(xSpacing, ySpacing));
        public static LayoutColumn XScale(this LayoutColumn column) =>
            new LayoutColumn(column, xScale: true);
        public static LayoutColumn YScale(this LayoutColumn column) =>
            new LayoutColumn(column, yScale: true);
        
        public static LayoutRow YScale(this LayoutRow row) =>
            new LayoutRow(row, yScale: true);
        public static LayoutRow Aligned(this LayoutRow row) =>
            new LayoutRow(row, aligned: true);
    }
}
