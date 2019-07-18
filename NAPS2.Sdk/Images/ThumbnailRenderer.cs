using System.Threading.Tasks;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;

namespace NAPS2.Images
{
    public class ThumbnailRenderer : IScannedImageRenderer<IImage>
    {
        // TODO: None of this static stuff is at all core SDK. Move it to FDesktop or something.
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

        private readonly ImageContext imageContext;
        private readonly IScannedImageRenderer<IImage> imageRenderer;

        public ThumbnailRenderer(ImageContext imageContext)
        {
            this.imageContext = imageContext;
            imageRenderer = new ImageRenderer(imageContext);
        }

        public ThumbnailRenderer(ImageContext imageContext, IScannedImageRenderer<IImage> imageRenderer)
        {
            this.imageContext = imageContext;
            this.imageRenderer = imageRenderer;
        }

        public async Task<IImage> Render(ScannedImage image, int outputSize)
        {
            using var snapshot = image.Preserve();
            return await Render(snapshot, outputSize);
        }

        public async Task<IImage> Render(ScannedImage.Snapshot snapshot, int outputSize)
        {
            using var bitmap = await imageRenderer.Render(snapshot, snapshot.Metadata.TransformList.Count == 0 ? 0 : outputSize * OVERSAMPLE);
            return imageContext.PerformTransform(bitmap, new ThumbnailTransform(outputSize));
        }
    }
}