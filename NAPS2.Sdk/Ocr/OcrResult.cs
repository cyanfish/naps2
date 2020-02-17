using System.Collections.Generic;
using System.Drawing;

namespace NAPS2.Ocr
{
    public class OcrResult
    {
        public OcrResult(Rectangle pageBounds, IEnumerable<OcrResultElement> elements, bool rightToLeft)
        {
            PageBounds = pageBounds;
            Elements = elements;
            RightToLeft = rightToLeft;
        }

        public Rectangle PageBounds { get; }

        public IEnumerable<OcrResultElement> Elements { get; }

        public bool RightToLeft { get; }
    }
}
