using System;

namespace NAPS2.Ocr
{
    public interface IOcrEngine
    {
        bool CanProcess(string langCode);

        OcrResult ProcessImage(string imagePath, string langCode, Func<bool> cancelCallback);
    }
}