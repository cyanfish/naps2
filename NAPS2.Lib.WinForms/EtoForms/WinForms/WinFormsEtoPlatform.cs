using System.Drawing.Imaging;
using System.Globalization;
using Eto;
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
using NAPS2.Platform.Windows;
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
    public override IDarkModeProvider DarkModeProvider { get; } = new WindowsDarkModeProvider();

    public override Application CreateApplicationCore()
    {
        WF.Application.EnableVisualStyles();
        WF.Application.SetCompatibleTextRenderingDefault(false);
        WF.Application.SetHighDpiMode(WF.HighDpiMode.PerMonitorV2);
        SetSystemTheme();
        return new Application(Eto.Platforms.WinForms);
    }

    public override void SetSystemTheme()
    {
        WF.Application.SetColorMode(ColorScheme.DarkMode ? WF.SystemColorMode.Dark : WF.SystemColorMode.Classic);
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        new WinFormsListView<T>(behavior);

    public override ISettingsNavigator CreateSettingsNavigator() => new WinFormsSettingsNavigator();

    public override void ConfigureImageButton(Button button, ButtonFlags flags)
    {
        if (string.IsNullOrEmpty(button.Text))
        {
            AttachDpiDependency(button, _ => button.MinimumSize = MinImageOnlyButtonSize);
            var native = (WF.Button) button.ToNative();
            native.TextImageRelation = WF.TextImageRelation.Overlay;
            native.ImageAlign = SD.ContentAlignment.MiddleCenter;
            return;
        }

        bool largeText = flags.HasFlag(ButtonFlags.LargeText);
        bool largeIcon = flags.HasFlag(ButtonFlags.LargeIcon);
        AttachDpiDependency(button, _ => button.MinimumSize = MinImageButtonSize);
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
                    button.Width = (int) Math.Round(width / scale);
                    var rightPadding = p + (width - widthWithoutRightPadding - p) / 2;
                    native.Padding = native.Padding with { Left = p, Right = rightPadding };
                }
            });
        }
    }

    public override void ConfigureDonateButton(Button button)
    {
        var native = (WF.Button) button.ToNative();
        native.FlatStyle = WF.FlatStyle.Flat;
        native.FlatAppearance.BorderColor = Color.FromRgb(0xfbad5f).ToSD();
        native.FlatAppearance.BorderSize = 2;
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
            using var hourglass = new SD.Bitmap(new MemoryStream(Icons.hourglass_grey_32));
            g.DrawImage(hourglass, new SD.Rectangle((bitmap.Width - 32) / 2, (bitmap.Height - 32) / 2, 32, 32));
        }
        image.Dispose();
        return new GdiImage(bitmap);
    }

    public override void SetFrame(Control container, Control control, Point location, Size size, bool inOverlay)
    {
        var native = control.ToNative();
        // Our layouts are in logical units like Eto's, so we scale them to pixels. Scaling the edges (rather than the
        // size) keeps adjacent controls from overlapping or leaving gaps.
        float scale = GetScale(native);
        int left = (int) Math.Round(location.X * scale);
        int top = (int) Math.Round(location.Y * scale);
        int width = (int) Math.Round((location.X + size.Width) * scale) - left;
        int height = (int) Math.Round((location.Y + size.Height) * scale) - top;
        if (CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
        {
            left = container.ToNative().Width - left - width;
        }
        native.AutoSize = false;
        native.SetBounds(left, top, width, height);
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
        float scale = GetScale(native);
        if (control.GetType() == typeof(Slider))
        {
            var size = ToLogical(native.PreferredSize, scale);
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
            ToLogical(native.PreferredSize, scale));
        if (control.GetType() == typeof(DropDown) && native.Height > 0)
        {
            // Work around a WinForms bug where the preferred height of a DropDown is incorrect
            preferredSize.Height = MathF.Ceiling(native.Height / scale);
        }
        return preferredSize;
    }

    public override SizeF GetWrappedSize(Control control, int defaultWidth)
    {
        if (control.ControlObject is WF.Label label)
        {
            float scale = GetScale(label);
            return ToLogical(WF.TextRenderer.MeasureText(label.Text, label.Font,
                new SD.Size((int) Math.Round(defaultWidth * scale), int.MaxValue),
                WF.TextFormatFlags.WordBreak), scale);
        }
        return base.GetWrappedSize(control, defaultWidth);
    }

    public override Size GetClientSize(Window window, bool excludeToolbars = false)
    {
        var form = window.ToNative();
        var size = form.ClientSize.ToEto();
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
        else if (excludeToolbars && window.ToolBar is { ControlObject: ToolStripEx toolStripEx })
        {
            size.Height -= toolStripEx.Height;
        }
        return Size.Round((SizeF) size / GetScale(form));
    }

    public override void SetClientSize(Window window, Size clientSize)
    {
        var native = window.ToNative();
        native.ClientSize = Size.Round(clientSize * GetScale(native)).ToSD();
        if (window is IFormBase { FormStateController.Loaded: false } form)
        {
            Invoker.Current.InvokeDispatch(() => form.LayoutController.Invalidate());
        }
    }

    public override void SetMinimumClientSize(Window window, Size minSize)
    {
        // Eto's ClientSize is in pixels before the window is loaded, so we use the native sizes
        var form = window.ToNative();
        var windowDecorationSize = form.Size - form.ClientSize;
        form.MinimumSize = Size.Round(minSize * GetScale(form)).ToSD() + windowDecorationSize;
    }

    private static float GetScale(WF.Control control) => control.DeviceDpi / 96f;

    // Rounding up means the size is at least as big as the native size when it's scaled back (otherwise e.g. text could
    // get cut off)
    private static SizeF ToLogical(SD.Size size, float scale) =>
        new(MathF.Ceiling(size.Width / scale), MathF.Ceiling(size.Height / scale));

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

    public override Control CreateScrollContainer()
    {
        var scrollPanel = new WF.Panel { AutoScroll = true };
        scrollPanel.Controls.Add(new WF.Panel());
        return scrollPanel.ToEto();
    }

    public override Control GetScrollContentContainer(Control scrollContainer)
    {
        return scrollContainer.ToNative().Controls[0].ToEto();
    }

    public override void SetScrollContentSize(Control scrollContainer, Size size)
    {
        var native = scrollContainer.ToNative();
        native.Controls[0].Size = Size.Round(size * GetScale(native)).ToSD();
    }

    public override int GetVerticalScrollBarWidth(Control scrollContainer)
    {
        var native = scrollContainer.ToNative();
        return (int) Math.Ceiling(WF.SystemInformation.GetVerticalScrollBarWidthForDpi(native.DeviceDpi) /
                                  GetScale(native));
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

        // Hide the window until it's fully laid out and painted, as WinForms shows it first (and maximizes it after)
        bool shown = false;
        form.HandleCreated += (_, _) =>
        {
            // The handle can be recreated later (e.g. if RightToLeft changes), but Shown is only fired once
            if (!shown) WindowCloaking.SetCloaked(form.Handle, true);
        };
        SetEnableRedrawDuringSuspend(window, true);
        window.Shown += (_, _) =>
        {
            shown = true;
            WindowCloaking.PaintNow(form.Handle);
            WindowCloaking.SetCloaked(form.Handle, false);
            SetEnableRedrawDuringSuspend(window, false);
        };
    }

    private static void SetEnableRedrawDuringSuspend(Window window, bool value)
    {
        switch (window.Handler)
        {
            case FormHandler formHandler:
                formHandler.EnableRedrawDuringSuspend = value;
                break;
            case DialogHandler dialogHandler:
                dialogHandler.EnableRedrawDuringSuspend = value;
                break;
        }
    }

    public override float GetScaleFactor(Window? window)
    {
        var form = window.ToNative();
        return form == null ? 1 : form.DeviceDpi / 96f;
    }

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

    public override void SetAccessibleName(Control control, string name) =>
        control.ToNative().AccessibleName = name;

    public override void ConfigureZoomButton(Button button, string icon)
    {
        button.Size = new Size(25, 25);
        AttachDpiDependency(button, scale =>
        {
            button.Image = IconProvider.GetIcon(icon, IconScenario.Button, scale);
        });
        var wfButton = (WF.Button) button.ToNative();
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

    public override byte[] GetData(IDataObject dataObject, string typeName)
    {
        var obj = dataObject as Widget;
        if (obj?.ControlObject is WF.DataObject wfObj)
        {
#pragma warning disable WFDEV005
            return (byte[]?) wfObj.GetData(typeName) ?? [];
#pragma warning restore WFDEV005
        }
        return base.GetData(dataObject, typeName);
    }

    public override void ConfigureDropDown(DropDown dropDown, bool scale)
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