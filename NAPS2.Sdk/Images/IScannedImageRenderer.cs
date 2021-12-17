using System.Threading.Tasks;

namespace NAPS2.Images;

public interface IScannedImageRenderer<TImage>
{
    Task<TImage> Render(ScannedImage image, int outputSize = 0);

    Task<TImage> Render(ScannedImage.Snapshot snapshot, int outputSize = 0);
}