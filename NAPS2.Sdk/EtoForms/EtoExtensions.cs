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

        public static LinkButton AsLink(this string linkText, Action? onClick = null) =>
            EtoHelpers.Link(linkText, onClick);

        public static Label NoWrap(this string labelText) => EtoHelpers.NoWrap(labelText);

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
    }
}
