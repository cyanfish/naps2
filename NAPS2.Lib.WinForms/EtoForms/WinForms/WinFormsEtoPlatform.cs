using System.Drawing.Imaging;
using System.Globalization;
using Eto.Drawing;
using Eto.Forms;
using Eto.WinForms;
using Eto.WinForms.Forms;
using Eto.WinForms.Forms.Controls;
using Eto.WinForms.Forms.Menu;
using Eto.WinForms.Forms.ToolBar;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Images.Gdi;
using SD = System.Drawing;
using WF = System.Windows.Forms;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsEtoPlatform : EtoPlatform
{
    private static readonly Size MinImageOnlyButtonSize = new(20, 20);
    private static readonly Size MinImageButtonSize = new(75, 32);
    private const int IMAGE_PADDING = 5;

    public override bool IsWinForms => true;

    public override IIconProvider IconProvider { get; } = new DefaultIconProvider();
    public override IDarkModeProvider DarkModeProvider { get; } = new WinFormsDarkModeProvider();

    public override Application CreateApplication()
    {
        WF.Application.EnableVisualStyles();
        WF.Application.SetCompatibleTextRenderingDefault(false);
        WF.Application.SetHighDpiMode(WF.HighDpiMode.PerMonitorV2);
        // WinForms dark mode is experimental
#pragma warning disable WFO5001
        WF.Application.SetColorMode(WF.SystemColorMode.System);
#pragma warning restore WFO5001
        return new Application(Eto.Platforms.WinForms);
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        new WinFormsListView<T>(behavior);

    public override void ConfigureImageButton(Button button, ButtonFlags flags)
    {
        if (string.IsNullOrEmpty(button.Text))
        {
            AttachDpiDependency(button, scale => button.MinimumSize = Size.Round(MinImageOnlyButtonSize * scale));
            var native = (WF.Button) button.ToNative();
            native.TextImageRelation = WF.TextImageRelation.Overlay;
            native.ImageAlign = SD.ContentAlignment.MiddleCenter;
            return;
        }

        bool largeText = flags.HasFlag(ButtonFlags.LargeText);
        bool largeIcon = flags.HasFlag(ButtonFlags.LargeIcon);
        AttachDpiDependency(button, scale => button.MinimumSize = Size.Round(MinImageButtonSize * scale));
        if (button.ImagePosition == ButtonImagePosition.Left)
        {
            var native = (WF.Button) button.ToNative()!;
            native.TextImageRelation = largeText ? WF.TextImageRelation.ImageBeforeText : WF.TextImageRelation.Overlay;
            native.ImageAlign = SD.ContentAlignment.MiddleLeft;
            native.TextAlign = largeText ? SD.ContentAlignment.MiddleLeft : SD.ContentAlignment.MiddleRight;

            if (largeText)
            {
                native.Text = @"  " + native.Text;
            }

            var imageWidth = largeIcon ? 32 : 16;
            native.AutoSize = false;

            AttachDpiDependency(button, scale =>
            {
                var textWidth = WF.TextRenderer.MeasureText(native.Text, native.Font).Width;
                int p = (int) Math.Round(IMAGE_PADDING * scale);
                if (largeText)
                {
                    native.Padding = native.Padding with { Left = p, Right = p };
                }
                else
                {
                    var widthWithoutRightPadding = p + textWidth + (int) Math.Round((imageWidth + 15) * scale);
                    var width = Math.Max(widthWithoutRightPadding + p,
                        (int) Math.Round(ButtonHandler.DefaultMinimumSize.Width * scale));
                    button.Width = width;
                    var rightPadding = p + (width - widthWithoutRightPadding - p) / 2;
                    native.Padding = native.Padding with { Left = p, Right = rightPadding };
                }
            });
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
        var bitmap = (SD.Bitmap) gdiImage.Bitmap.Clone();
        return bitmap.ToEto();
    }

    public override IMemoryImage FromBitmap(Bitmap bitmap)
    {
        return new GdiImage((SD.Bitmap) bitmap.ToSD());
    }

    public override IMemoryImage DrawHourglass(IMemoryImage image)
    {
        var bitmap = new System.Drawing.Bitmap(image.Width, image.Height);
        using (var g = SD.Graphics.FromImage(bitmap))
        {
            var attrs = new ImageAttributes();
            attrs.SetColorMatrix(new ColorMatrix
            {
                Matrix33 = 0.3f
            });
            g.DrawImage(image.AsBitmap(),
                new SD.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                0,
                0,
                image.Width,
                image.Height,
                SD.GraphicsUnit.Pixel,
                attrs);
            using var hourglass = new SD.Bitmap(new MemoryStream(Icons.hourglass_grey));
            g.DrawImage(hourglass, new SD.Rectangle((bitmap.Width - 32) / 2, (bitmap.Height - 32) / 2, 32, 32));
        }
        image.Dispose();
        return new GdiImage(bitmap);
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
        native.Location = new SD.Point(x, y);
        native.AutoSize = false;
        native.Size = new SD.Size(size.Width, size.Height);
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
        var native = control.ToNative();
        if (control.GetType() == typeof(Slider))
        {
            var size = (SizeF) native.PreferredSize.ToEto();
            // Work around a WinForms bug where the preferred size of a Slider/WF.TrackBar is based on the primary
            // screen DPI, not the DPI of the screen it's actually on
            var scaleDelta = (native.FindForm()!.DeviceDpi / 96f) / (Screen.PrimaryScreen.RealDPI / 72f);
            size *= scaleDelta;
            // We also want to correct for the idea that sliders should be fully scalable in orthogonal directions
            // depending on the orientation
            return ((Slider) control).Orientation == Orientation.Horizontal
                ? new SizeF(size.Height * scaleDelta, size.Height * scaleDelta)
                : new SizeF(size.Width * scaleDelta, size.Width * scaleDelta);
        }
        var preferredSize = SizeF.Max(
            base.GetPreferredSize(control, availableSpace),
            native.PreferredSize.ToEto());
        if (control.GetType() == typeof(DropDown) && control.Height > 0)
        {
            // Work around a WinForms bug where the preferred height of a DropDown is incorrect
            preferredSize.Height = control.Height;
        }
        return preferredSize;
    }

    public override SizeF GetWrappedSize(Control control, int defaultWidth)
    {
        if (control.ControlObject is WF.Label label)
        {
            return WF.TextRenderer.MeasureText(label.Text, label.Font, new SD.Size(defaultWidth, int.MaxValue),
                WF.TextFormatFlags.WordBreak).ToEto();
        }
        return base.GetWrappedSize(control, defaultWidth);
    }

    public override Size GetClientSize(Window window, bool excludeToolbars = false)
    {
        var size = window.ToNative().ClientSize.ToEto();
        if (excludeToolbars && window.Content is { ControlObject: WF.ToolStripContainer container })
        {
            var top = container.TopToolStripPanel.Controls.Cast<WF.ToolStrip>();
            var bottom = container.BottomToolStripPanel.Controls.Cast<WF.ToolStrip>();
            var left = container.LeftToolStripPanel.Controls.Cast<WF.ToolStrip>();
            var right = container.RightToolStripPanel.Controls.Cast<WF.ToolStrip>();
            size -= new Size(
                left.Concat(right).Sum(x => x.Width),
                top.Concat(bottom).Sum(x => x.Height));
        }
        return size;
    }

    public override void SetClientSize(Window window, Size clientSize)
    {
        window.ToNative().ClientSize = clientSize.ToSD();
        if (window is IFormBase { FormStateController.Loaded: false } form)
        {
            Invoker.Current.InvokeDispatch(() => form.LayoutController.Invalidate());
        }
    }

    public override Control CreateContainer()
    {
        return new WF.Panel().ToEto();
    }

    public override void AddToContainer(Control container, Control control, bool inOverlay)
    {
        if (control.ToNative() is WF.TextBox textBox)
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

    public override void InitForm(Window window)
    {
        var form = window.ToNative();

        bool isRtl = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
        form.RightToLeft = isRtl ? WF.RightToLeft.Yes : WF.RightToLeft.No;
        form.RightToLeftLayout = isRtl;

        form.DpiChanged += (_, _) => (window as IFormBase)?.LayoutController.Invalidate();
    }

    public override float GetScaleFactor(Window window)
    {
        var form = window.ToNative();
        // Force creation of form handle
        _ = form.Handle;
        return form.DeviceDpi / 96f;
    }

    public override bool ScaleLayout => true;

    public override void SetImageSize(ButtonMenuItem menuItem, int size)
    {
        var handler = (ButtonMenuItemHandler) menuItem.Handler;
        handler.ImageSize = size;
    }

    public override void SetImageSize(ToolItem menuItem, int size)
    {
        if (menuItem.Handler is ButtonToolItemHandler buttonHandler)
        {
            buttonHandler.ImageSize = size;
        }
        if (menuItem.Handler is DropDownToolItemHandler dropDownHandler)
        {
            dropDownHandler.ImageSize = size;
        }
    }

    public override void ConfigureZoomButton(Button button, string icon)
    {
        AttachDpiDependency(button, scale =>
        {
            button.Size = new Size((int) (25 * scale), (int) (25 * scale));
            button.Image = IconProvider.GetIcon(icon, scale);
        });
        var wfButton = (WF.Button) button.ToNative();
        wfButton.AccessibleName = button.Text;
        wfButton.Text = "";
        wfButton.BackColor = ColorScheme.BackgroundColor.ToSD();
        wfButton.FlatStyle = WF.FlatStyle.Flat;
    }

    private void AttachDpiDependency(WF.Control control, Action<float> callback)
    {
        void DpiChanged(object? sender, EventArgs eventArgs)
        {
            callback(GetScaleFactor(control.FindForm().ToEtoWindow()));
        }
        void Register()
        {
            if (control is WF.Form form)
            {
                form.DpiChanged += DpiChanged;
            }
            else
            {
                control.DpiChangedAfterParent += DpiChanged;
            }
            DpiChanged(null, EventArgs.Empty);
        }
        void Unregister()
        {
            if (control is WF.Form form)
            {
                form.DpiChanged -= DpiChanged;
            }
            else
            {
                control.DpiChangedAfterParent -= DpiChanged;
            }
        }

        if (control.IsHandleCreated)
        {
            Register();
        }
        else
        {
            control.HandleCreated += (_, _) => Register();
        }
        control.HandleDestroyed += (_, _) => Unregister();
    }

    public override void AttachDpiDependency(Control control, Action<float> callback) =>
        AttachDpiDependency(control.ToNative(), callback);

    public override void SetClipboardImage(Clipboard clipboard, ProcessedImage processedImage, IMemoryImage memoryImage)
    {
        // We also add the JPEG/PNG format to the clipboard as some applications care about the actual format
        // https://github.com/cyanfish/naps2/issues/264
        var jpegOrPng = ImageExportHelper.SaveSmallestFormatToMemoryStream(memoryImage,
            processedImage.Metadata.Lossless, -1, out var fileFormat);
        var handler = (ClipboardHandler) clipboard.Handler;
        // Note this only updates the DataObject, it doesn't set the clipboard, that's done below
        handler.Control.SetData(fileFormat == ImageFileFormat.Jpeg ? "JFIF" : "PNG", jpegOrPng);

        if (memoryImage.PixelFormat is ImagePixelFormat.BW1 or ImagePixelFormat.Gray8)
        {
            // Storing 1bit/8bit images to the clipboard doesn't work, so we copy to 24bit if needed
            using var memoryImage2 = memoryImage.CopyWithPixelFormat(ImagePixelFormat.RGB24);
            handler.Control.SetImage(memoryImage2.AsBitmap());
        }
        else
        {
            handler.Control.SetImage(memoryImage.AsBitmap());
        }
        WF.Clipboard.SetDataObject(handler.Control, true);
    }

    public override void ConfigureDropDown(DropDown dropDown)
    {
        ((WF.ComboBox) dropDown.ControlObject).DrawMode = WF.DrawMode.Normal;
    }

    public override void ShowIcon(Dialog dialog)
    {
        ((WF.Form) dialog.ControlObject).ShowIcon = true;
    }

    public override void ConfigureEllipsis(Label label)
    {
        var handler = (LabelHandler) label.Handler;
        handler.Control.AutoEllipsis = true;
    }

    public override Bitmap? ExtractAssociatedIcon(string exePath)
    {
        return SD.Icon.ExtractAssociatedIcon(exePath)?.ToBitmap().ToEto();
    }

    public override void HandleKeyDown(Control control, Func<Keys, bool> handle)
    {
        control.ToNative().KeyDown += (_, args) => args.Handled = handle(args.KeyData.ToEto());
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
            var wfControl = control.ToNative();
            wfControl.MouseWheel += (sender, e) => eventHandler(sender, e.ToEto(wfControl));
        }
    }

    public override void AttachMouseMoveEvent(Control control, EventHandler<MouseEventArgs> eventHandler)
    {
        var wfControl = control.ToNative();
        wfControl.MouseMove += (sender, e) => eventHandler(sender, e.ToEto(wfControl));
    }

    private class ScrollableWithMouseWheelEvents : ScrollableHandler.CustomScrollable
    {
        private readonly EventHandler<MouseEventArgs> _mouseWheelHandler;

        public ScrollableWithMouseWheelEvents(ScrollableHandler handler, EventHandler<MouseEventArgs> mouseWheelHandler)
        {
            _mouseWheelHandler = mouseWheelHandler;

            // TODO: Fix this in Eto so we don't need a custom class
            Handler = handler;
            Size = SD.Size.Empty;
            MinimumSize = SD.Size.Empty;
            BorderStyle = WF.BorderStyle.Fixed3D;
            AutoScroll = true;
            AutoSize = true;
            AutoSizeMode = WF.AutoSizeMode.GrowAndShrink;
            VerticalScroll.SmallChange = 5;
            VerticalScroll.LargeChange = 10;
            HorizontalScroll.SmallChange = 5;
            HorizontalScroll.LargeChange = 10;
            Controls.Add(handler.ContainerContentControl);
        }

        protected override void OnMouseWheel(WF.MouseEventArgs e)
        {
            var etoArgs = e.ToEto(this);
            _mouseWheelHandler.Invoke(this, etoArgs);
            if (e is WF.HandledMouseEventArgs handledArgs)
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