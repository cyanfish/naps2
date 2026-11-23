using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Widgets;

public class ScrollZoomImageViewer
{
    private readonly Scrollable _scrollable = new() { Border = BorderType.None };
    private readonly Drawable _imageView = new();
    private float _renderFactor;
    private PointF? _mousePos;
    private PointF? _initialMousePos;
    private Point? _initialScrollPos;
    private bool _mouseIsDown;
    private bool _fitToContainer = true;

    public ScrollZoomImageViewer()
    {
        _scrollable.Content = _imageView;
        _imageView.Paint += ImagePaint;
        _imageView.MouseDown += (_, e) => OverlayMouseDown?.Invoke(this, e);
        _imageView.MouseMove += (_, e) => OverlayMouseMove?.Invoke(this, e);
        _imageView.MouseUp += (_, e) => OverlayMouseUp?.Invoke(this, e);
        _scrollable.MouseEnter += OnMouseEnter;
        _scrollable.MouseLeave += OnMouseLeave;
        _scrollable.MouseMove += OnMouseMove;
        _scrollable.MouseDown += OnMouseDown;
        _scrollable.MouseUp += OnMouseUp;
        EtoPlatform.Current.AttachMouseWheelEvent(_scrollable, OnMouseWheel);
        _scrollable.SizeChanged += OnSizeChanged;
        _scrollable.Scroll += (_, _) => _imageView.Invalidate();
        _scrollable.Cursor = Cursors.Pointer;
    }

    // For some reason Gtk can't render a few pixels width around the edge of the image, so we need to correct for that.
    private int BorderOffset { get; } = EtoPlatform.Current.IsGtk ? 6 : 0;

    /// <summary>
    /// The image shown. The viewer doesn't own it; use SetImage to replace it with the zoom adjusted.
    /// </summary>
    public Bitmap? Image { get; private set; }

    public ColorScheme? ColorScheme { get; set; }

    public bool PanEnabled { get; set; } = true;

    public bool FitIsMinZoom { get; set; }

    public int ImagePadding
    {
        get;
        set
        {
            field = value;
            if (Image != null)
            {
                RenderSize = RenderSize;
            }
        }
    }

    /// <summary>
    /// The image's bounds in the coordinate space used by PaintOverlay and the overlay mouse events.
    /// </summary>
    public RectangleF ImageRect => new(XOffset, YOffset, RenderSize.Width, RenderSize.Height);

    public Cursor? OverlayCursor
    {
        set => _imageView.Cursor = value;
    }

    public event EventHandler<ZoomChangedEventArgs>? ZoomChanged;

    public event EventHandler<PaintEventArgs>? PaintOverlay;

    public event EventHandler<MouseEventArgs>? OverlayMouseDown;
    public event EventHandler<MouseEventArgs>? OverlayMouseMove;
    public event EventHandler<MouseEventArgs>? OverlayMouseUp;

    private Size RenderSize
    {
        get;
        set
        {
            field = value;
            _imageView.Size = Size.Round(value) + new Size(2 + 2 * ImagePadding, 2 + 2 * ImagePadding);
            _imageView.Invalidate();
        }
    }

    private int AvailableWidth => _scrollable.Width - 2 - BorderOffset - 2 * ImagePadding;
    private int AvailableHeight => _scrollable.Height - 2 - BorderOffset - 2 * ImagePadding;

    /// <summary>
    /// Replaces the image. If the new image has the same shape as the old one, it's shown at the same size on screen
    /// (so a preview can be updated in place); otherwise it's zoomed to fit.
    /// </summary>
    public void SetImage(Bitmap? image)
    {
        var old = Image;
        Image = image;
        if (image == null)
        {
            RenderSize = Size.Empty;
            _renderFactor = 0;
            return;
        }
        bool sameShape = old != null && RenderSize.Width > 0 &&
                         Math.Abs(old.Width / (float) old.Height - image.Width / (float) image.Height) < 0.01f;
        if (sameShape)
        {
            _renderFactor = RenderSize.Width / (float) image.Width;
            RenderSize = Size.Round(new SizeF(image.Width * _renderFactor, image.Height * _renderFactor));
            ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(_renderFactor));
        }
        else
        {
            ZoomToContainer();
        }
    }

    public void Invalidate() => _imageView.Invalidate();

    private bool IsWidthBound =>
        Image!.Width / (float) Image.Height > AvailableWidth / (float) AvailableHeight;

    private float FitFactor =>
        IsWidthBound ? AvailableWidth / (float) Image!.Width : AvailableHeight / (float) Image!.Height;

    private bool FitsInBounds => RenderSize.Width <= AvailableWidth && RenderSize.Height <= AvailableHeight;

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
        if (PanEnabled && _mouseIsDown && _initialMousePos.HasValue && _initialScrollPos.HasValue)
        {
            _scrollable.ScrollPosition = _initialScrollPos.Value + Point.Round(_initialMousePos.Value - e.Location);
        }
        _mousePos = e.Location;
    }

    private void OnMouseLeave(object? sender, MouseEventArgs e)
    {
        _mousePos = null;
    }

    // An image that was fitted to the viewer stays fitted when the viewer's size changes (the window is resized, or
    // other controls appear next to it); an explicitly chosen zoom level is kept.
    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (!_fitToContainer || Image == null) return;
        if (EtoPlatform.Current.IsGtk)
        {
            // Gtk delays adjusting the image view size after the container size changes
            Invoker.Current.InvokeDispatch(() =>
            {
                if (_fitToContainer) ZoomToContainer();
            });
        }
        else
        {
            ZoomToContainer();
        }
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
        if (Image != null)
        {
            e.Graphics.DrawRectangle(
                ColorScheme?.BorderColor ?? Colors.Black,
                XOffset - 1, YOffset - 1, RenderSize.Width + 1, RenderSize.Height + 1);
            DrawVisiblePart(e.Graphics, Image);
            PaintOverlay?.Invoke(this, e);
        }
    }

    private void DrawVisiblePart(Graphics graphics, Bitmap image)
    {
        float scaleX = RenderSize.Width / (float) image.Width;
        float scaleY = RenderSize.Height / (float) image.Height;
        if (scaleX <= 1)
        {
            graphics.DrawImage(image, XOffset, YOffset, RenderSize.Width, RenderSize.Height);
            return;
        }
        var visible = _scrollable.VisibleRect;
        float left = Math.Max((float) Math.Floor((visible.Left - XOffset) / scaleX), 0);
        float top = Math.Max((float) Math.Floor((visible.Top - YOffset) / scaleY), 0);
        float right = Math.Min((float) Math.Ceiling((visible.Right - XOffset) / scaleX), image.Width);
        float bottom = Math.Min((float) Math.Ceiling((visible.Bottom - YOffset) / scaleY), image.Height);
        if (right <= left || bottom <= top) return;
        graphics.DrawImage(image,
            new RectangleF(left, top, right - left, bottom - top),
            new RectangleF(XOffset + left * scaleX, YOffset + top * scaleY,
                (right - left) * scaleX, (bottom - top) * scaleY));
    }

    public float ZoomFactor => _renderFactor;

    // GDI doesn't like when the render size (4 bytes per pixel) exceeds int.MaxValue
    public float MaxZoom => (float) Math.Sqrt(int.MaxValue * 0.99 / (Image!.Width * Image.Height * 4));

    public void ChangeZoom(float step, bool anchorToMouse = false)
    {
        SetZoom(_renderFactor * (float) Math.Pow(1.2, step), anchorToMouse);
    }

    public void SetZoom(float value, bool anchorToMouse = false)
    {
        if (FitIsMinZoom && value <= FitFactor)
        {
            ZoomToContainer();
            return;
        }
        _fitToContainer = false;
        _renderFactor = value.Clamp(0.01f, MaxZoom);
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
        _fitToContainer = false;
        RenderSize = new Size(Image!.Width, Image.Height);
        _renderFactor = 1f;
        ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(_renderFactor));
    }

    /// <summary>
    /// Fits the image to the viewer, now and whenever the viewer's size changes (until the zoom is changed). If the
    /// viewer doesn't have a size yet, the image is fitted once it does.
    /// </summary>
    public void ZoomToContainer()
    {
        _fitToContainer = true;
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
        var scrollableMiddle = new PointF(_scrollable.Width / 2f, _scrollable.Height / 2f);
        if (!anchorToMouse ||
            _mousePos is not { } mousePos ||
            mousePos.X < 0 ||
            mousePos.Y < 0 ||
            mousePos.X > _scrollable.Width ||
            mousePos.Y > _scrollable.Height)
        {
            // Mouse is outside the scrollable
            return (anchorMiddle, scrollableMiddle);
        }
        var mouseRelativePos = mousePos - new Point(1, 1);
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
        if (FitsInBounds)
        {
            return;
        }
        // TODO: This is off a bit for the "middle" anchor, probably because the scrollbars themselves appear
        var xScroll = anchor.imageAnchor.X * RenderSize.Width + XOffset - anchor.mouseRelativePos.X;
        var yScroll = anchor.imageAnchor.Y * RenderSize.Height + YOffset - anchor.mouseRelativePos.Y;
        _scrollable.ScrollPosition = Point.Round(new PointF(xScroll, yScroll));
    }

    public static implicit operator LayoutElement(ScrollZoomImageViewer control)
    {
        return control._scrollable;
    }

    public LayoutControl Scale() => _scrollable.Scale().NaturalSize(0, 0);
}