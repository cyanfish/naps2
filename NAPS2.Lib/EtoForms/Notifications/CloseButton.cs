using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Notifications;

public class CloseButton : Drawable
{
    private const int CLOSE_BUTTON_PADDING = 5;

    private readonly ColorScheme _colorScheme;

    private bool _hover;
    private bool _active;

    public CloseButton(ColorScheme colorScheme)
    {
        _colorScheme = colorScheme;
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

    private Color PenColor => _colorScheme.NotificationBorderColor;
    private Color DefaultBackground => _colorScheme.NotificationBackgroundColor;
    private Color HoverBackground => Color.Blend(
        _colorScheme.NotificationBackgroundColor,
        _colorScheme.NotificationBorderColor,
        0.3f);
    private Color ActiveBackground => Color.Blend(
        _colorScheme.NotificationBackgroundColor,
        _colorScheme.NotificationBorderColor,
        0.6f);

    private void OnPaint(object? sender, PaintEventArgs e)
    {
        var bgColor = _active && _hover ? ActiveBackground : _hover ? HoverBackground : DefaultBackground;
        var w = Width;
        var h = Height;
        e.Graphics.FillRectangle(bgColor, 0, 0, w, h);
        var p = CLOSE_BUTTON_PADDING;
        var pen = new Pen(PenColor, 3);
        e.Graphics.DrawLine(pen, p - 1, p - 1, w - p, h - p);
        e.Graphics.DrawLine(pen, w - p, p - 1, p - 1, h - p);
    }

    public event EventHandler? Click;
}