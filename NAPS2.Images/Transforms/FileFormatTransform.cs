// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms;

/// <summary>
/// Sets the image's original file format without changing its pixels. This can affect how the image is exported, e.g.
/// setting JPEG allows a PNG image to be exported with lossy compression (as long as it isn't marked as lossless).
/// </summary>
public record FileFormatTransform : Transform
{
    public FileFormatTransform()
    {
    }

    public FileFormatTransform(ImageFileFormat format)
    {
        Format = format;
    }

    public ImageFileFormat Format { get; private set; }

    public override bool IsNull => Format == ImageFileFormat.Unknown;

    public override bool CanSimplify(Transform other) => other is FileFormatTransform;

    public override Transform Simplify(Transform other) => this;
}
