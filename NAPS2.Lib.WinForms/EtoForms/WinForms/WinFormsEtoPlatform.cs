using System.Drawing;
using System.Windows.Forms;
using Eto.Forms;
using Eto.WinForms.Forms.Controls;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsEtoPlatform : EtoPlatform
{
    private const int MIN_BUTTON_WIDTH = 75;
    private const int MIN_BUTTON_HEIGHT = 32;
    private const int IMAGE_PADDING = 5;
    
    static WinFormsEtoPlatform()
    {
        ButtonHandler.DefaultMinimumSize = new Eto.Drawing.Size(MIN_BUTTON_WIDTH, MIN_BUTTON_HEIGHT);
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        new WinFormsListView<T>(behavior);

    public override void ConfigureImageButton(Eto.Forms.Button button)
    {
        if (button.ImagePosition == ButtonImagePosition.Left)
        {
            var native = (System.Windows.Forms.Button) button.ToNative();
            native.TextImageRelation = TextImageRelation.Overlay;
            native.ImageAlign = ContentAlignment.MiddleLeft;
            native.TextAlign = ContentAlignment.MiddleRight;

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
}