using System.Drawing.Imaging;
using System.Globalization;
using Eto.Drawing;
using Eto.Forms;
using Eto.WinForms;
using Eto.WinForms.Forms.Controls;
using NAPS2.EtoForms.Layout;
using NAPS2.Images.Gdi;
using sd = System.Drawing;
using wf = System.Windows.Forms;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsEtoPlatform : EtoPlatform
{
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

    public override void ConfigureImageButton(Eto.Forms.Button button)
    {
        button.MinimumSize = MinImageButtonSize;
        if (button.ImagePosition == ButtonImagePosition.Left)
        {
            var native = (wf.Button) button.ToNative();
            native.TextImageRelation = wf.TextImageRelation.Overlay;
            native.ImageAlign = sd.ContentAlignment.MiddleLeft;
            native.TextAlign = sd.ContentAlignment.MiddleRight;

            var imageWidth = native.Image.Width;
            using var g = native.CreateGraphics();
            var textWidth = (int) g.MeasureString(native.Text, native.Font).Width;
            native.AutoSize = false;

            var widthWithoutRightPadding = imageWidth + textWidth + IMAGE_PADDING + 15;
            native.Width = Math.Max(widthWithoutRightPadding + IMAGE_PADDING, ButtonHandler.DefaultMinimumSize.Width);
            var rightPadding = IMAGE_PADDING + (native.Width - widthWithoutRightPadding - IMAGE_PADDING) / 2;
            native.Padding = native.Padding with { Left = IMAGE_PADDING, Right = rightPadding };
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
        return ((GdiImage) image).Bitmap.ToEto();
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
            return GetPreferredSize(((Panel) control).Content, availableSpace);
        }
        return SizeF.Max(
            base.GetPreferredSize(control, availableSpace),
            control.ToNative().PreferredSize.ToEto());
    }

    public override SizeF GetWrappedSize(Control control, int defaultWidth)
    {
        if (control.ControlObject is wf.Label label)
        {
            label.AutoSize = false;
            label.Width = 0;
            return label.GetPreferredSize(new sd.Size(defaultWidth, 0)).ToEto();
        }
        return base.GetWrappedSize(control, defaultWidth);
    }

    public override Control CreateContainer()
    {
        return new wf.Panel().ToEto();
    }

    public override void AddToContainer(Control container, Control control, bool inOverlay)
    {
        container.ToNative().Controls.Add(control.ToNative());
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

    public override void SetClipboardImage(Clipboard clipboard, Bitmap image)
    {
        var dataObj = (wf.DataObject) clipboard.ControlObject;
        dataObj.SetImage(image.ToSD());
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
}