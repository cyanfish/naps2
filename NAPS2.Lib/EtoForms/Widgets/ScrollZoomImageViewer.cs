using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Widgets;

public class ScrollZoomImageViewer
{
    private readonly Scrollable _scrollable = new() { Border = BorderType.None };
    private readonly Drawable _imageView = new() { BackgroundColor = Colors.White };
    private Size _renderSize;
    private float _renderFactor;
    private PointF? _mousePos;
    private PointF? _initialMousePos;
    private Point? _initialScrollPos;
    private bool _mouseIsDown;

    public ScrollZoomImageViewer()
    {
        _scrollable.Content = _imageView;
        _scrollable.BackgroundColor = Colors.White;
        _imageView.Paint += ImagePaint;
        _scrollable.MouseEnter += OnMouseEnter;
        _scrollable.MouseLeave += OnMouseLeave;
        _scrollable.MouseMove += OnMouseMove;
        _scrollable.MouseDown += OnMouseDown;
        _scrollable.MouseUp += OnMouseUp;
        EtoPlatform.Current.AttachMouseWheelEvent(_scrollable, OnMouseWheel);
        _scrollable.Cursor = Cursors.Pointer;
    }

    // For some reason Gtk can't render a few pixels width around the edge of the image, so we need to correct for that.
    private int BorderOffset { get; } = EtoPlatform.Current.IsGtk ? 6 : 0;

    public Bitmap? Image { get; set; }

    public ColorScheme? ColorScheme { get; set; }

    public event EventHandler<ZoomChangedEventArgs>? ZoomChanged;

    private Size RenderSize
    {
        get => _renderSize;
        set
        {
            _renderSize = value;
            _imageView.Size = Size.Round(value) + new Size(2, 2);
            _imageView.Invalidate();
        }
    }

    private int AvailableWidth => _scrollable.Width - 2 - BorderOffset;
    private int AvailableHeight => _scrollable.Height - 2 - BorderOffset;

    private bool IsWidthBound =>
        Image!.Width / (float) Image.Height > AvailableWidth / (float) AvailableHeight;

    private float XOffset => Math.Max((_imageView.Width - RenderSize.Width) / 2, 0);
    private float YOffset => Math.Max((_imageView.Height - RenderSize.Height) / 2, 0);


    private void OnMouseEnter(object? sender, MouseEventArgs e)
    {
        // TODO: Mouse enter/leave events aren't firing on WinForms, why?
        _mousePos = e.Location;
    }

    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        _mouseIsDown = false;
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        _mouseIsDown = true;
        _initialMousePos = e.Location;
        _initialScrollPos = _scrollable.ScrollPosition;
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (_mouseIsDown && _initialMousePos.HasValue && _initialScrollPos.HasValue)
        {
            _scrollable.ScrollPosition = _initialScrollPos.Value + Point.Round(_initialMousePos.Value - e.Location);
        }
        _mousePos = e.Location;
    }

    private void OnMouseLeave(object? sender, MouseEventArgs e)
    {
        _mousePos = null;
    }

    private void OnMouseWheel(object? sender, MouseEventArgs e)
    {
        if (e.Modifiers == Keys.Control)
        {
            ChangeZoom(e.Delta.Height, true);
            e.Handled = true;
        }
    }

    private void ImagePaint(object? sender, PaintEventArgs e)
    {
        e.Graphics.SetClip(e.ClipRectangle);
        e.Graphics.Clear(ColorScheme?.BackgroundColor ?? Colors.White);
        if (Image != null)
        {
            e.Graphics.DrawRectangle(
                ColorScheme?.BorderColor ?? Colors.Black,
                XOffset - 1, YOffset - 1, RenderSize.Width + 1, RenderSize.Height + 1);
            e.Graphics.DrawImage(Image, XOffset, YOffset, RenderSize.Width, RenderSize.Height);
        }
    }

    public float ZoomFactor => _renderFactor;

    public void ChangeZoom(float step, bool anchorToMouse = false)
    {
        SetZoom(_renderFactor * (float) Math.Pow(1.2, step), anchorToMouse);
    }

    public void SetZoom(float value, bool anchorToMouse = false)
    {
        // TODO: Adjust clamp values based on image size
        // (and also propagate that to the Mac slider limits)
        _renderFactor = value.Clamp(0.01f, 10);
        _scrollable.SuspendLayout();
        var anchor = GetMouseAnchor(anchorToMouse);
        RenderSize =
            Size.Round(new SizeF(Image!.Width * _renderFactor, Image.Height * _renderFactor));
        SetMouseAnchor(anchor);
        _scrollable.ResumeLayout();
        ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(_renderFactor));
    }

    public void ZoomToActual()
    {
        RenderSize = new Size(Image!.Width, Image.Height);
        _renderFactor = 1f;
        ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(_renderFactor));
    }

    public void ZoomToContainer()
    {
        if (!_scrollable.Loaded || Image == null || _scrollable.Width <= 0 || _scrollable.Height <= 0)
        {
            return;
        }
        if (IsWidthBound)
        {
            RenderSize = new Size(AvailableWidth,
                (int) Math.Round(Image!.Height * AvailableWidth / (float) Image.Width));
            _renderFactor = AvailableWidth / (float) Image.Width;
        }
        else
        {
            RenderSize =
                new Size((int) Math.Round(Image!.Width * AvailableHeight / (float) Image.Height),
                    AvailableHeight);
            _renderFactor = AvailableHeight / (float) Image.Height;
        }
        ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(_renderFactor));
    }

    // When we zoom in/out (e.g. with Ctrl+mousewheel), we want the point in the image underneath the mouse cursor to
    // stay stationary relative to the mouse (as much as permitted by the available scroll region). If the mouse is not
    // overtop the image, the middle of the image (as currently visible) should stay stationary.
    //
    // This function calculates the image anchor as a fraction (i.e. a point in the range [<0,0>, <1,1>]). It also
    // returns the mouse position relative to the top left of the Scrollable (or the middle of the scrollable if the
    // mouse is not overtop the image).
    private (PointF imageAnchor, PointF mouseRelativePos) GetMouseAnchor(bool anchorToMouse)
    {
        var anchorMiddle = new PointF(0.5f, 0.5f);
        var scrollableMiddle = new PointF(
            _scrollable.Location.X + _scrollable.Width / 2,
            _scrollable.Location.Y + _scrollable.Height / 2);
        if (!anchorToMouse ||
            _mousePos is not { } mousePos ||
            mousePos.X < _scrollable.Location.X ||
            mousePos.Y < _scrollable.Location.Y ||
            mousePos.X > _scrollable.Location.X + _scrollable.Width ||
            mousePos.Y > _scrollable.Location.Y + _scrollable.Height)
        {
            // Mouse is outside the scrollable
            return (anchorMiddle, scrollableMiddle);
        }
        var mouseRelativePos = mousePos - _scrollable.Location - new Point(1, 1);
        var x = (mouseRelativePos.X + _scrollable.ScrollPosition.X - XOffset) / RenderSize.Width;
        var y = (mouseRelativePos.Y + _scrollable.ScrollPosition.Y - YOffset) / RenderSize.Height;
        if (x < 0 || y < 0 || x > 1 || y > 1)
        {
            // Mouse is inside the scrollable but outside the area covered by the image
            return (anchorMiddle, scrollableMiddle);
        }
        return (new PointF(x, y), mouseRelativePos);
    }

    // This function inverts the calculation done in GetMouseAnchor to get the correct scroll position to move the point
    // in the image that was underneath the mouse back there (after the image size has been changed).
    private void SetMouseAnchor((PointF imageAnchor, PointF mouseRelativePos) anchor)
    {
        // TODO: This is off a bit for the "middle" anchor, probably because the scrollbars themselves appear
        var xScroll = anchor.imageAnchor.X * RenderSize.Width + XOffset - anchor.mouseRelativePos.X;
        var yScroll = anchor.imageAnchor.Y * RenderSize.Height + YOffset - anchor.mouseRelativePos.Y;
        _scrollable.ScrollPosition = Point.Round(new PointF(xScroll, yScroll));
    }


    public static implicit operator LayoutElement(ScrollZoomImageViewer control)
    {
        return control._scrollable;
    }
}