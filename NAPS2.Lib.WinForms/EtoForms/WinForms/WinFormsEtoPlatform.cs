using System.Drawing.Imaging;
using System.Globalization;
using Eto.Drawing;
using Eto.Forms;
using Eto.WinForms;
using Eto.WinForms.Forms.Controls;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Images.Gdi;
using sd = System.Drawing;
using wf = System.Windows.Forms;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsEtoPlatform : EtoPlatform
{
    private static readonly Size MinImageOnlyButtonSize = new(20, 20);
    private static readonly Size MinImageButtonSize = new(75, 32);
    private const int IMAGE_PADDING = 5;

    public override bool IsWinForms => true;

    public override Application CreateApplication()
    {
        wf.Application.EnableVisualStyles();
        wf.Application.SetCompatibleTextRenderingDefault(false);
        return new Application(Eto.Platforms.WinForms);
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        new WinFormsListView<T>(behavior);

    public override void ConfigureImageButton(Button button, bool big)
    {
        if (string.IsNullOrEmpty(button.Text))
        {
            button.MinimumSize = MinImageOnlyButtonSize;
            var native = (wf.Button) button.ToNative();
            native.TextImageRelation = wf.TextImageRelation.Overlay;
            native.ImageAlign = sd.ContentAlignment.MiddleCenter;
            return;
        }

        button.MinimumSize = MinImageButtonSize;
        if (button.ImagePosition == ButtonImagePosition.Left)
        {
            var native = (wf.Button) button.ToNative();
            native.TextImageRelation = big ? wf.TextImageRelation.ImageBeforeText : wf.TextImageRelation.Overlay;
            native.ImageAlign = sd.ContentAlignment.MiddleLeft;
            native.TextAlign = big ? sd.ContentAlignment.MiddleLeft : sd.ContentAlignment.MiddleRight;

            if (big)
            {
                native.Text = @"  " + native.Text;
            }

            var imageWidth = native.Image.Width;
            using var g = native.CreateGraphics();
            var textWidth = (int) g.MeasureString(native.Text, native.Font).Width;
            native.AutoSize = false;

            if (big)
            {
                native.Padding = native.Padding with { Left = IMAGE_PADDING, Right = IMAGE_PADDING };
            }
            else
            {
                var widthWithoutRightPadding = imageWidth + textWidth + IMAGE_PADDING + 15;
                button.Width = Math.Max(widthWithoutRightPadding + IMAGE_PADDING,
                    ButtonHandler.DefaultMinimumSize.Width);
                var rightPadding = IMAGE_PADDING + (native.Width - widthWithoutRightPadding - IMAGE_PADDING) / 2;
                native.Padding = native.Padding with { Left = IMAGE_PADDING, Right = rightPadding };
            }
        }
    }

    public override Control AccessibleImageButton(Image image, string text, Action onClick,
        int xOffset = 0, int yOffset = 0)
    {
        // This works by overlaying an image on top a button.
        // If the image has transparency an offset may need to be specified to keep the button hidden.
        // If the text is too large relative to the button it will be impossible to hide fully.
        var imageView = new ImageView { Image = image, Cursor = Eto.Forms.Cursors.Pointer };
        imageView.MouseDown += (_, _) => onClick();
        var button = new Button
        {
            Text = text,
            Width = 0,
            Height = 0,
            Command = new ActionCommand(onClick)
        };
        var pix = new PixelLayout();
        pix.Add(button, xOffset, yOffset);
        pix.Add(imageView, 0, 0);
        return pix;
    }

    public override Bitmap ToBitmap(IMemoryImage image)
    {
        var gdiImage = (GdiImage) image;
        var bitmap = (sd.Bitmap) gdiImage.Bitmap.Clone();
        return bitmap.ToEto();
    }

    public override IMemoryImage DrawHourglass(ImageContext imageContext, IMemoryImage image)
    {
        var bitmap = new System.Drawing.Bitmap(image.Width, image.Height);
        using (var g = sd.Graphics.FromImage(bitmap))
        {
            var attrs = new ImageAttributes();
            attrs.SetColorMatrix(new ColorMatrix
            {
                Matrix33 = 0.3f
            });
            g.DrawImage(image.AsBitmap(),
                new sd.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                0,
                0,
                image.Width,
                image.Height,
                sd.GraphicsUnit.Pixel,
                attrs);
            using var hourglass = new sd.Bitmap(new MemoryStream(Icons.hourglass_grey));
            g.DrawImage(hourglass, new sd.Rectangle((bitmap.Width - 32) / 2, (bitmap.Height - 32) / 2, 32, 32));
        }
        image.Dispose();
        return new GdiImage(imageContext, bitmap);
    }

    public override void SetFrame(Control container, Control control, Point location, Size size, bool inOverlay)
    {
        var native = control.ToNative();
        var x = location.X;
        var y = location.Y;
        if (CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
        {
            x = container.Width - x - size.Width;
        }
        native.Location = new sd.Point(x, y);
        native.AutoSize = false;
        native.Size = new sd.Size(size.Width, size.Height);
        if (inOverlay)
        {
            native.BringToFront();
        }
    }

    public override SizeF GetPreferredSize(Control control, SizeF availableSpace)
    {
        if (control.GetType() == typeof(Panel))
        {
            var content = ((Panel) control).Content;
            if (content != null)
            {
                return GetPreferredSize(content, availableSpace);
            }
        }
        return SizeF.Max(
            base.GetPreferredSize(control, availableSpace),
            control.ToNative().PreferredSize.ToEto());
    }

    public override SizeF GetWrappedSize(Control control, int defaultWidth)
    {
        if (control.ControlObject is wf.Label label)
        {
            using var g = label.CreateGraphics();
            return g.MeasureString(label.Text, label.Font, defaultWidth).ToEto();
        }
        return base.GetWrappedSize(control, defaultWidth);
    }

    public override Control CreateContainer()
    {
        return new wf.Panel().ToEto();
    }

    public override void AddToContainer(Control container, Control control, bool inOverlay)
    {
        if (control.ToNative() is wf.TextBox textBox)
        {
            // WinForms textboxes behave weirdly when resized during load and can push text offscreen. This fixes that.
            control.Load += (_, _) =>
            {
                textBox.Select(0, 0);
                textBox.ScrollToCaret();
            };
        }
        container.ToNative().Controls.Add(control.ToNative());
    }

    public override void RemoveFromContainer(Control container, Control control)
    {
        container.ToNative().Controls.Remove(control.ToNative());
    }

    public override LayoutElement FormatProgressBar(ProgressBar progressBar)
    {
        return progressBar.Size(420, 40);
    }

    public override void UpdateRtl(Window window)
    {
        var form = window.ToNative();
        bool isRtl = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
        form.RightToLeft = isRtl ? wf.RightToLeft.Yes : wf.RightToLeft.No;
        form.RightToLeftLayout = isRtl;
    }

    public override void ConfigureZoomButton(Button button)
    {
        button.Size = new Size(23, 23);
        var wfButton = (wf.Button) button.ToNative();
        wfButton.AccessibleName = button.Text;
        wfButton.Text = "";
        wfButton.BackColor = sd.Color.White;
        wfButton.FlatStyle = wf.FlatStyle.Flat;
    }

    public override void ConfigureDropDown(DropDown dropDown)
    {
        ((wf.ComboBox) dropDown.ControlObject).DrawMode = wf.DrawMode.Normal;
    }

    public override void ShowIcon(Dialog dialog)
    {
        ((wf.Form) dialog.ControlObject).ShowIcon = true;
    }

    public override void ConfigureEllipsis(Label label)
    {
        var handler = (LabelHandler) label.Handler;
        handler.Control.AutoEllipsis = true;
    }

    public override Bitmap? ExtractAssociatedIcon(string exePath)
    {
        return sd.Icon.ExtractAssociatedIcon(exePath)?.ToBitmap().ToEto();
    }

    public override void AttachMouseWheelEvent(Control control, EventHandler<MouseEventArgs> eventHandler)
    {
        if (control is Scrollable scrollable)
        {
            var content = scrollable.Content;
            var border = scrollable.Border;
            var wfControl = new ScrollableWithMouseWheelEvents((ScrollableHandler) scrollable.Handler, eventHandler);
            ((ScrollableHandler) control.Handler).Control = wfControl;
            scrollable.Content = content;
            scrollable.Border = border;
        }
        else
        {
            throw new NotImplementedException("Only implemented for Scrollable");
        }
    }

    private class ScrollableWithMouseWheelEvents : ScrollableHandler.CustomScrollable
    {
        private readonly EventHandler<MouseEventArgs> _mouseWheelHandler;

        public ScrollableWithMouseWheelEvents(ScrollableHandler handler, EventHandler<MouseEventArgs> mouseWheelHandler)
        {
            _mouseWheelHandler = mouseWheelHandler;

            // TODO: Fix this in Eto so we don't need a custom class
            Handler = handler;
            Size = sd.Size.Empty;
            MinimumSize = sd.Size.Empty;
            BorderStyle = wf.BorderStyle.Fixed3D;
            AutoScroll = true;
            AutoSize = true;
            AutoSizeMode = wf.AutoSizeMode.GrowAndShrink;
            VerticalScroll.SmallChange = 5;
            VerticalScroll.LargeChange = 10;
            HorizontalScroll.SmallChange = 5;
            HorizontalScroll.LargeChange = 10;
            Controls.Add(handler.ContainerContentControl);
        }

        protected override void OnMouseWheel(wf.MouseEventArgs e)
        {
            var etoArgs = e.ToEto(this);
            _mouseWheelHandler.Invoke(this, etoArgs);
            if (e is wf.HandledMouseEventArgs handledArgs)
            {
                handledArgs.Handled |= etoArgs.Handled;
            }
            if (!etoArgs.Handled)
            {
                base.OnMouseWheel(e);
            }
        }
    }
}