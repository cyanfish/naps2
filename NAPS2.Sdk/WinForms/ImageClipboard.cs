using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images;

namespace NAPS2.WinForms
{
    public class ImageClipboard
    {
        private readonly BitmapRenderer bitmapRenderer;

        public ImageClipboard()
        {
            bitmapRenderer = new BitmapRenderer();
        }

        public ImageClipboard(BitmapRenderer bitmapRenderer)
        {
            this.bitmapRenderer = bitmapRenderer;
        }
    }
}
