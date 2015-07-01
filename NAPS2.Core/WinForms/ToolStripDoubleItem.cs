using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class ToolStripDoubleItem : ToolStripItem
    {
        private readonly ToolStripItem top;
        private readonly ToolStripItem bottom;

        private Rectangle imageRect;
        private Rectangle textRect;

        public ToolStripDoubleItem(ToolStripItem top, ToolStripItem bottom)
        {
            this.top = top;
            this.bottom = bottom;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.Owner != null)
            {
                // Find the dimensions of the image and the text  
                // areas of the item.  
                this.ComputeImageAndTextLayout();

                // Draw the background. This includes drawing a highlighted  
                // border when the mouse is in the client area.
                ToolStripItemRenderEventArgs ea = new ToolStripItemRenderEventArgs(
                     e.Graphics,
                     this);
                this.Owner.Renderer.DrawItemBackground(ea);

                // Draw the item's image. 
                ToolStripItemImageRenderEventArgs irea =
                    new ToolStripItemImageRenderEventArgs(
                    e.Graphics,
                    this,
                    imageRect);
                this.Owner.Renderer.DrawItemImage(irea);

                // Draw the text, and highlight it if the  
                // the rollover state is true.
                ToolStripItemTextRenderEventArgs rea =
                    new ToolStripItemTextRenderEventArgs(
                    e.Graphics,
                    this,
                    base.Text,
                    textRect,
                    base.ForeColor,
                    base.Font,
                    base.TextAlign);
                this.Owner.Renderer.DrawItemText(rea);
            }
        }

        private void ComputeImageAndTextLayout()
        {
            Rectangle cr = base.ContentRectangle;
            Image img = Image;

            // Compute the center of the item's ContentRectangle. 
            int centerY = (cr.Height - img.Height) / 2;

            // Find the dimensions of the image and the text  
            // areas of the item. The text occupies the space  
            // not filled by the image.  
            if (base.TextImageRelation == TextImageRelation.ImageBeforeText &&
                base.TextDirection == ToolStripTextDirection.Horizontal)
            {
                imageRect = new Rectangle(
                    base.ContentRectangle.Left,
                    centerY,
                    base.Image.Width,
                    base.Image.Height);

                textRect = new Rectangle(
                    imageRect.Width,
                    base.ContentRectangle.Top,
                    base.ContentRectangle.Width - imageRect.Width,
                    base.ContentRectangle.Height);
            }
            else if (base.TextImageRelation == TextImageRelation.TextBeforeImage &&
                     base.TextDirection == ToolStripTextDirection.Horizontal)
            {
                imageRect = new Rectangle(
                    base.ContentRectangle.Right - base.Image.Width,
                    centerY,
                    base.Image.Width,
                    base.Image.Height);

                textRect = new Rectangle(
                    base.ContentRectangle.Left,
                    base.ContentRectangle.Top,
                    imageRect.X,
                    base.ContentRectangle.Bottom);
            }
        }
    }
}
