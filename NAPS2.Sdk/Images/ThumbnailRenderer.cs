using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Config;

namespace NAPS2.Images
{
    public class ThumbnailRenderer
    {
        public const int MIN_SIZE = 64;
        public const int DEFAULT_SIZE = 128;
        public static int MAX_SIZE = 1024;

        private const int OVERSAMPLE = 3;

        public static double StepNumberToSize(double stepNumber)
        {
            // 64-256:32:6 256-448:48:4 448-832:64:6 832-1024:96:2
            if (stepNumber < 6)
            {
                return 64 + stepNumber * 32;
            }
            if (stepNumber < 10)
            {
                return 256 + (stepNumber - 6) * 48;
            }
            if (stepNumber < 16)
            {
                return 448 + (stepNumber - 10) * 64;
            }
            return 832 + (stepNumber - 16) * 96;
        }

        public static double SizeToStepNumber(double size)
        {
            if (size < 256)
            {
                return (size - 64) / 32;
            }
            if (size < 448)
            {
                return (size - 256) / 48 + 6;
            }
            if (size < 832)
            {
                return (size - 448) / 64 + 10;
            }
            return (size - 832) / 96 + 16;
        }

        //public Task<Bitmap> RenderThumbnail(ScannedImage scannedImage, int size)
        //{
        //    using (var snapshot = scannedImage.Preserve())
        //    {
        //        return RenderThumbnail(snapshot, size);
        //    }
        //}

        //public async Task<Bitmap> RenderThumbnail(ScannedImage.Snapshot snapshot, int size)
        //{
        //    using (var bitmap = await scannedImageRenderer.Render(snapshot, snapshot.TransformList.Count == 0 ? 0 : size * OVERSAMPLE))
        //    {
        //        return RenderThumbnail(bitmap, size);
        //    }
        //}
    }
}