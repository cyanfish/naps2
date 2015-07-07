using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class ToolStripDoubleButton : ToolStripButton
    {
        private int currentButton = -1;

        public ToolStripDoubleButton()
        {
        }

        public event EventHandler ClickFirst;
        public event EventHandler ClickSecond;

        public Image ImageFirst { get; set; }
        public Image ImageSecond { get; set; }

        public string TextFirst { get; set; }
        public string TextSecond { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            // base.OnPaint(e);
            if (Owner == null)
                return;
            ToolStripRenderer renderer = new ToolStripSystemRenderer();

            var oldHeight = Height;
            var oldParent = Parent;
            Parent = null;
            Height = Height / 2;
            e.Graphics.TranslateTransform(0, currentButton == 1 ? Height : 0);
            renderer.DrawButtonBackground(new ToolStripItemRenderEventArgs(e.Graphics, this));
            e.Graphics.TranslateTransform(0, currentButton == 1 ? -Height : 0);
            Height = oldHeight;
            Parent = oldParent;

            if (ImageFirst != null && TextFirst != null)
            {
                e.Graphics.DrawImage(ImageFirst, new Point(Padding.Left, Height / 4 - ImageFirst.Height / 2));

                var textRectangle = new Rectangle(Padding.Left + ImageFirst.Width, 0, Width, Height / 2);
                renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(e.Graphics, this, TextFirst, textRectangle, ForeColor, Font, TextFormatFlags.Left | TextFormatFlags.VerticalCenter));
            }

            if (ImageSecond != null && TextSecond != null)
            {
                e.Graphics.DrawImage(ImageSecond, new Point(Padding.Left, Height * 3 / 4 - ImageSecond.Height / 2));

                var textRectangle = new Rectangle(Padding.Left + ImageSecond.Width, Height / 2, Width, Height / 2);
                renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(e.Graphics, this, TextSecond, textRectangle, ForeColor, Font, TextFormatFlags.Left | TextFormatFlags.VerticalCenter));
            }

            Image = null;
        }

        protected override void OnMouseMove(MouseEventArgs mea)
        {
            base.OnMouseMove(mea);
            var oldCurrentButton = currentButton;
            currentButton = mea.Y > (Height / 2) ? 1 : 0;
            if (currentButton != oldCurrentButton)
            {
                Invalidate();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (currentButton == 0)
            {
                if (ClickFirst != null)
                {
                    ClickFirst.Invoke(this, e);
                }
            }
            else if (currentButton == 1)
            {
                if (ClickSecond != null)
                {
                    ClickSecond.Invoke(this, e);
                }
            }
        }
    }
}
