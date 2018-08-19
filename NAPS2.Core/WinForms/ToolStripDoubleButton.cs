using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Platform;

namespace NAPS2.WinForms
{
    public class ToolStripDoubleButton : ToolStripButton
    {
        private int currentButton = -1;

        public ToolStripDoubleButton()
        {
        }

        public event EventHandler FirstClick;
        public event EventHandler SecondClick;

        public Image FirstImage { get; set; }
        public Image SecondImage { get; set; }

        [Localizable(true)]
        public string FirstText { get; set; }
        [Localizable(true)]
        public string SecondText { get; set; }

        public int MaxTextWidth { get; set; }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            bool wrap = false;
            var sumWidth = Padding.Left + Padding.Right
                           + Math.Max(FirstImage?.Width ?? 0, SecondImage?.Width ?? 0)
                           + Math.Max(MeasureTextWidth(FirstText, ref wrap), MeasureTextWidth(SecondText, ref wrap));
            var sumHeight = Padding.Top + Padding.Bottom
                            + (FirstImage?.Height ?? 0) + (SecondImage?.Height ?? 0)
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
            
            if (PlatformCompat.Runtime.UseToolStripRenderHack)
            {
                var oldHeight = Height;
                var oldParent = Parent;
                Parent = null;
                Height = Height / 2;
                e.Graphics.TranslateTransform(0, currentButton == 1 ? Height : 0);
                renderer.DrawButtonBackground(new ToolStripItemRenderEventArgs(e.Graphics, this));
                e.Graphics.TranslateTransform(0, currentButton == 1 ? -Height : 0);
                Height = oldHeight;
                Parent = oldParent;
            }
            else
            {
                if (currentButton == 0)
                {
                    e.Graphics.DrawRectangle(new Pen(Color.Black), 0, 0, Width - 1, Height / 2 - 1);
                }
                if (currentButton == 1)
                {
                    e.Graphics.DrawRectangle(new Pen(Color.Black), 0, Height / 2, Width - 1, Height / 2 - 1);
                }
            }

            bool wrap = false;
            int textWidth = Math.Max(MeasureTextWidth(FirstText, ref wrap), MeasureTextWidth(SecondText, ref wrap));
            var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
            if (wrap)
            {
                flags |= TextFormatFlags.WordBreak;
            }

            if (FirstImage != null && FirstText != null)
            {
                if (Enabled)
                {
                    e.Graphics.DrawImage(FirstImage, new Point(Padding.Left, Height / 4 - FirstImage.Height / 2));
                }
                else
                {
                    ControlPaint.DrawImageDisabled(e.Graphics, FirstImage, Padding.Left, Height / 4 - FirstImage.Height / 2, Color.Transparent);
                }

                var textRectangle = new Rectangle(Padding.Left + FirstImage.Width, 0, textWidth, Height / 2);
                renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(e.Graphics, this, FirstText, textRectangle, ForeColor, Font, flags));
            }

            if (SecondImage != null && SecondText != null)
            {
                if (Enabled)
                {
                    e.Graphics.DrawImage(SecondImage, new Point(Padding.Left, Height * 3 / 4 - SecondImage.Height / 2));
                }
                else
                {
                    ControlPaint.DrawImageDisabled(e.Graphics, SecondImage, Padding.Left, Height * 3 / 4 - SecondImage.Height / 2, Color.Transparent);
                }

                var textRectangle = new Rectangle(Padding.Left + SecondImage.Width, Height / 2, textWidth, Height / 2);
                renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(e.Graphics, this, SecondText, textRectangle, ForeColor, Font, flags));
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

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            currentButton = -1;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (currentButton == 0)
            {
                FirstClick?.Invoke(this, e);
            }
            else if (currentButton == 1)
            {
                SecondClick?.Invoke(this, e);
            }
        }
    }
}
