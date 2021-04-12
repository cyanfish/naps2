using Eto.Forms;

namespace NAPS2.EtoForms
{
    public class ControlWithLayoutAttributes
    {
        public ControlWithLayoutAttributes(Control control)
        {
            Control = control;
        }
        
        public ControlWithLayoutAttributes(ControlWithLayoutAttributes control, bool? center = null, bool? xScale = null, bool? yScale = null, bool? autoSize = null)
        {
            Control = control.Control;
            Center = center ?? control.Center;
            XScale = xScale ?? control.XScale;
            YScale = yScale ?? control.YScale;
            AutoSize = autoSize ?? control.AutoSize;
        }
        
        public static implicit operator ControlWithLayoutAttributes(Control control)
        {
            return new ControlWithLayoutAttributes(control);
        }
        
        public static implicit operator ControlWithLayoutAttributes(string label)
        {
            return new ControlWithLayoutAttributes(new Label { Text = label });
        }
        
        public Control Control { get; }
        public bool Center { get; }
        public bool XScale { get; }
        public bool YScale { get; }
        public bool AutoSize { get; }

        public void AddTo(DynamicLayout layout)
        {
            if (AutoSize)
            {
                layout.AddAutoSized(Control, xscale: XScale, yscale: YScale, centered: Center);
            }
            else if (Center)
            {
                layout.AddCentered(Control, xscale: XScale, yscale: YScale);
            }
            else
            {
                layout.Add(Control, xscale: XScale, yscale: YScale);
            }
        }
    }
}
