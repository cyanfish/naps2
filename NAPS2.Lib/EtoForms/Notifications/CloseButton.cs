using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Notifications;

public class CloseButton : Drawable
{
    private const int CLOSE_BUTTON_PADDING = 5;
    private static readonly Pen CloseButtonPen = new(new Color(0.7f, 0.7f, 0.7f), 3);
    private static readonly Color DefaultBackground = new(0.95f, 0.95f, 0.95f);
    private static readonly Color ActiveBackground = new(0.8f, 0.8f, 0.8f);
    private static readonly Color HoverBackground = new(0.87f, 0.87f, 0.87f);

    private bool _hover;
    private bool _active;

    public CloseButton()
    {
        Cursor = Cursors.Pointer;
        Paint += OnPaint;
        MouseEnter += (_, _) =>
        {
            _hover = true;
            Invalidate();
        };
        MouseLeave += (_, _) =>
        {
            _hover = false;
            Invalidate();
        };
        MouseDown += (_, _) =>
        {
            _active = true;
            Invalidate();
        };
        MouseUp += (_, _) =>
        {
            var actualHover = new Rectangle(0, 0, Width, Height).Contains(Point.Round(PointFromScreen(Mouse.Position)));
            if (_active && _hover && actualHover)
            {
                Click?.Invoke(this, EventArgs.Empty);
            }
            _active = false;
            Invalidate();
        };
    }

    private void OnPaint(object? sender, PaintEventArgs e)
    {
        var clearColor = _active && _hover ? ActiveBackground : _hover ? HoverBackground : DefaultBackground;
        e.Graphics.Clear(clearColor);
        var w = e.ClipRectangle.Width;
        var h = e.ClipRectangle.Height;
        var p = CLOSE_BUTTON_PADDING;
        e.Graphics.DrawLine(CloseButtonPen, p - 1, p - 1, w - p, h - p);
        e.Graphics.DrawLine(CloseButtonPen, w - p, p - 1, p - 1, h - p);
    }

    public event EventHandler? Click;
}