using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NAPS2.WinForms;

public class ToolStripDoubleButton : ToolStripButton
{
    private int _currentButton = -1;

    public event EventHandler? FirstClick;
    public event EventHandler? SecondClick;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Image? FirstImage { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Image? SecondImage { get; set; }

    [Localizable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public required string FirstText { get; init; }

    [Localizable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public required string SecondText { get; init; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int MaxTextWidth { get; init; }

    public override Size GetPreferredSize(Size constrainingSize)
    {
        bool wrap = false;
        var sumWidth = Padding.Left + Padding.Right
                                    + Math.Max(FirstImage?.Width ?? 0, SecondImage?.Width ?? 0)
                                    + Math.Max(MeasureTextWidth(FirstText, ref wrap),
                                        MeasureTextWidth(SecondText, ref wrap));
        var sumHeight = Padding.Top + Padding.Bottom
                                    + (FirstImage?.Height ?? 0) + (SecondImage?.Height ?? 0)
                                    + 16 + (wrap ? 12 : 0);
        return new Size(sumWidth, sumHeight);
    }

    private int MeasureTextWidth(string text, ref bool wrap)
    {
        var width = TextRenderer.MeasureText(text, Font).Width;
        if (MaxTextWidth > 0 && width > MaxTextWidth)
        {
            var words = text.Split(' ');
            for (int i = 1; i < words.Length; i++)
            {
                var left = string.Join(" ", words.Take(words.Length - i));
                var right = string.Join(" ", words.Skip(words.Length - i));
                var wrappedWidth = TextRenderer.MeasureText(left + "\n" + right, Font).Width;
                if (wrappedWidth < width)
                {
                    width = wrappedWidth;
                    wrap = true;
                }
            }
        }
        return width;
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
        e.Graphics.TranslateTransform(0, _currentButton == 1 ? Height : 0);
        renderer.DrawButtonBackground(new ToolStripItemRenderEventArgs(e.Graphics, this));
        e.Graphics.TranslateTransform(0, _currentButton == 1 ? -Height : 0);
        Height = oldHeight;
        Parent = oldParent;

        bool wrap = false;
        int textWidth = Math.Max(MeasureTextWidth(FirstText, ref wrap), MeasureTextWidth(SecondText, ref wrap));
        var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
        if (wrap)
        {
            flags |= TextFormatFlags.WordBreak;
        }

        if (Enabled)
        {
            e.Graphics.DrawImage(FirstImage, new Point(Padding.Left, Height / 4 - FirstImage.Height / 2 + 1));
        }
        else
        {
            ControlPaint.DrawImageDisabled(e.Graphics, FirstImage, Padding.Left, Height / 4 - FirstImage.Height / 2 + 1,
                Color.Transparent);
        }
        var textRectangle1 = new Rectangle(Padding.Left + FirstImage.Width, 0, textWidth, Height / 2);
        renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(e.Graphics, this, FirstText, textRectangle1,
            ForeColor, Font, flags));

        if (Enabled)
        {
            e.Graphics.DrawImage(SecondImage, new Point(Padding.Left, Height * 3 / 4 - SecondImage.Height / 2));
        }
        else
        {
            ControlPaint.DrawImageDisabled(e.Graphics, SecondImage, Padding.Left,
                Height * 3 / 4 - SecondImage.Height / 2, Color.Transparent);
        }
        var textRectangle2 = new Rectangle(Padding.Left + SecondImage.Width, Height / 2, textWidth, Height / 2);
        renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(e.Graphics, this, SecondText, textRectangle2,
            ForeColor, Font, flags));

        Image = null;
    }

    protected override void OnMouseMove(MouseEventArgs mea)
    {
        base.OnMouseMove(mea);
        var oldCurrentButton = _currentButton;
        _currentButton = mea.Y > (Height / 2) ? 1 : 0;
        if (_currentButton != oldCurrentButton)
        {
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _currentButton = -1;
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);

        if (_currentButton == 0)
        {
            FirstClick?.Invoke(this, e);
        }
        else if (_currentButton == 1)
        {
            SecondClick?.Invoke(this, e);
        }
    }
}