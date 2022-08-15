namespace NAPS2.Images;

public interface IRenderableImage
{
    IImageStorage Storage { get; }
    TransformState TransformState { get; }
}