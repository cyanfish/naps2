using System.Drawing;

namespace NAPS2.Ocr
{
    public class OcrResultElement
    {
        public OcrResultElement(string text, Rectangle bounds)
        {
            Text = text;
            Bounds = bounds;
        }

        public string Text { get; }

        public Rectangle Bounds { get; }
    }
}
