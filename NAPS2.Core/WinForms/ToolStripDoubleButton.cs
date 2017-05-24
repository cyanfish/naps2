using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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

        [Localizable(true)]
        public string TextFirst { get; set; }
        [Localizable(true)]
        public string TextSecond { get; set; }

        public int MaxTextWidth { get; set; }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            bool wrap = false;
            var sumWidth = Padding.Left + Padding.Right +
                           Math.Max(ImageFirst != null ? ImageFirst.Width : 0,
                               ImageSecond != null ? ImageSecond.Width : 0)
                           + Math.Max(MeasureTextWidth(TextFirst, ref wrap), MeasureTextWidth(TextSecond, ref wrap));
            var sumHeight = Padding.Top + Padding.Bottom +
                           (ImageFirst != null ? ImageFirst.Height : 0)
                           + (ImageSecond != null ? ImageSecond.Height : 0)
                           + 16 + (wrap ? 12 : 0);
            return new Size(sumWidth, sumHeight);
        }

        private int MeasureTextWidth(string text, ref bool wrap)
        {
            using (var g = Graphics.FromImage(new Bitmap(1, 1)))
            {
                var width = (int)Math.Ceiling(g.MeasureString(text, Font).Width);
                if (MaxTextWidth > 0 && width > MaxTextWidth)
                {
                    var words = text.Split(' ');
                    for (int i = 1; i < words.Length; i++)
                    {
                        var left = string.Join(" ", words.Take(words.Length - i));
                        var right = string.Join(" ", words.Skip(words.Length - i));
                        var wrappedWidth = (int)Math.Ceiling(g.MeasureString(left + "\n" + right, Font).Width);
                        if (wrappedWidth < width)
                        {
                            width = wrappedWidth;
                            wrap = true;
                        }
                    }
                }
                return width;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Owner == null)
                return;
            ToolStripRenderer renderer = ToolStripManager.Renderer;

            var oldHeight = Height;
            var oldParent = Parent;
            Parent = null;
            Height = Height / 2;
            e.Graphics.TranslateTransform(0, currentButton == 1 ? Height : 0);
            renderer.DrawButtonBackground(new ToolStripItemRenderEventArgs(e.Graphics, this));
            e.Graphics.TranslateTransform(0, currentButton == 1 ? -Height : 0);
            Height = oldHeight;
            Parent = oldParent;

            bool wrap = false;
            int textWidth = Math.Max(MeasureTextWidth(TextFirst, ref wrap), MeasureTextWidth(TextSecond, ref wrap));
            var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
            if (wrap)
            {
                flags |= TextFormatFlags.WordBreak;
            }

            if (ImageFirst != null && TextFirst != null)
            {
                if (Enabled)
                {
                    e.Graphics.DrawImage(ImageFirst, new Point(Padding.Left, Height / 4 - ImageFirst.Height / 2));
                }
                else
                {
                    ControlPaint.DrawImageDisabled(e.Graphics, ImageFirst, Padding.Left, Height / 4 - ImageFirst.Height / 2, Color.Transparent);
                }

                var textRectangle = new Rectangle(Padding.Left + ImageFirst.Width, 0, textWidth, Height / 2);
                renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(e.Graphics, this, TextFirst, textRectangle, ForeColor, Font, flags));
            }

            if (ImageSecond != null && TextSecond != null)
            {
                if (Enabled)
                {
                    e.Graphics.DrawImage(ImageSecond, new Point(Padding.Left, Height * 3 / 4 - ImageSecond.Height / 2));
                }
                else
                {
                    ControlPaint.DrawImageDisabled(e.Graphics, ImageSecond, Padding.Left, Height * 3 / 4 - ImageSecond.Height / 2, Color.Transparent);
                }

                var textRectangle = new Rectangle(Padding.Left + ImageSecond.Width, Height / 2, textWidth, Height / 2);
                renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(e.Graphics, this, TextSecond, textRectangle, ForeColor, Font, flags));
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
                ClickFirst?.Invoke(this, e);
            }
            else if (currentButton == 1)
            {
                ClickSecond?.Invoke(this, e);
            }
        }
    }
}
