using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Notifications;

public abstract class Notification : IDisposable
{
    // TODO: Get from color scheme
    protected static readonly Color BackgroundColor = new(0.95f, 0.95f, 0.95f);
    private static readonly Color BorderColor = new(0.7f, 0.7f, 0.7f);
    private const int BORDER_RADIUS = 7;
    private const int CLOSE_BUTTON_SIZE = 18;

    protected const int HIDE_LONG = 60 * 1000;
    protected const int HIDE_SHORT = 5 * 1000;

    protected int HideTimeout { get; set; }

    protected bool ShowClose { get; set; } = true;

    public NotificationManager? Manager { get; set; }

    protected abstract LayoutElement PrimaryContent { get; }

    protected abstract LayoutElement SecondaryContent { get; }

    public LayoutElement CreateView()
    {
        var drawable = new Drawable();
        drawable.Paint += DrawableOnPaint;
        var closeButton = new CloseButton();
        closeButton.Click += (_, _) => Manager!.Hide(this);
        drawable.MouseUp += (_, _) => NotificationClicked();
        drawable.Load += (_, _) => SetUpHideTimeout(drawable);
        return L.Overlay(
            drawable,
            L.Column(
                L.Row(
                    PrimaryContent,
                    ShowClose ? C.Spacer().Width(CLOSE_BUTTON_SIZE) : C.None()),
                SecondaryContent
            ).Padding(10, 8, 10, 8),
            ShowClose
                ? L.Column(
                    L.Row(
                        C.Filler(),
                        closeButton.Size(CLOSE_BUTTON_SIZE, CLOSE_BUTTON_SIZE)
                    ),
                    C.Filler()
                ).Padding(5, 5, 5, 5)
                : C.None());
    }

    private void SetUpHideTimeout(Control control)
    {
        Timer CreateTimer()
        {
            return new Timer(_ =>
            {
                Manager!.Hide(this);
            }, null, HideTimeout, -1);
        }

        if (HideTimeout > 0)
        {
            // Don't start the timer until the user is interacting with the window
            void StartTimer(object? sender, EventArgs e)
            {
                Manager!.TimersStarting -= StartTimer;
                control.ParentWindow.MouseMove -= StartTimer;
                var timer = CreateTimer();
                control.MouseEnter += (_, _) => timer.Dispose();
                control.MouseLeave += (_, _) => timer = CreateTimer();
            }
            Manager!.TimersStarting += StartTimer;
        }
    }

    protected virtual void NotificationClicked()
    {
    }

    private static void DrawableOnPaint(object? sender, PaintEventArgs e)
    {
        var w = e.ClipRectangle.Width;
        var h = e.ClipRectangle.Height;
        e.Graphics.FillRectangle(BackgroundColor, 0, 0, w, h);
        e.Graphics.DrawRectangle(BorderColor, 0, 0, w - 1, h - 1);
    }

    private static void DrawWithRoundedCorners(PaintEventArgs e)
    {
        // TODO: We're not using this as the few pixels on the edges aren't transparent, which is a problem if there's
        // an image underneath. Not sure if there's a way to make that work but I don't care enough about rounded
        // corners at the moment.
        var w = e.ClipRectangle.Width;
        var h = e.ClipRectangle.Height;
        var r = BORDER_RADIUS;
        var d = r * 2;
        var q = r / 2;
        // TODO: Color scheme background
        e.Graphics.Clear(Colors.White);
        // Corners
        e.Graphics.FillEllipse(BackgroundColor, -1, -1, d, d);
        e.Graphics.FillEllipse(BackgroundColor, w - d, -1, d, d);
        e.Graphics.FillEllipse(BackgroundColor, -1, h - d, d, d);
        e.Graphics.FillEllipse(BackgroundColor, w - d, h - d, d, d);
        // Corner borders
        e.Graphics.DrawEllipse(BorderColor, -1, -1, d, d);
        e.Graphics.DrawEllipse(BorderColor, w - d, -1, d, d);
        e.Graphics.DrawEllipse(BorderColor, -1, h - d, d, d);
        e.Graphics.DrawEllipse(BorderColor, w - d, h - d, d, d);
        // Middle
        e.Graphics.FillRectangle(BackgroundColor, r, r, w - d, h - d);
        // Sides
        e.Graphics.FillRectangle(BackgroundColor, 0, r, r, h - d);
        e.Graphics.FillRectangle(BackgroundColor, r, 0, w - d, r);
        e.Graphics.FillRectangle(BackgroundColor, w - r, r, r, h - d);
        e.Graphics.FillRectangle(BackgroundColor, r, h - r, w - d, r);
        // Side borders
        e.Graphics.DrawLine(BorderColor, 0, q, 0, h - q);
        e.Graphics.DrawLine(BorderColor, q, 0, w - q, 0);
        e.Graphics.DrawLine(BorderColor, w - 1, q, w - 1, h - q);
        e.Graphics.DrawLine(BorderColor, q, h - 1, w - q, h - 1);
    }

    public virtual void Dispose()
    {
    }
}