using System.Collections.Generic;

namespace NAPS2.Ocr
{
    public class OcrResult
    {
        public OcrResult((int x, int y, int w, int h) pageBounds, IEnumerable<OcrResultElement> elements, bool rightToLeft)
        {
            PageBounds = pageBounds;
            Elements = elements;
            RightToLeft = rightToLeft;
        }

        public (int x, int y, int w, int h) PageBounds { get; }

        public IEnumerable<OcrResultElement> Elements { get; }

        public bool RightToLeft { get; }
    }
}
