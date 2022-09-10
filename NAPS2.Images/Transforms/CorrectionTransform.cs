namespace NAPS2.Images.Transforms;

// TODO: experimental
public class CorrectionTransform : Transform
{
    public CorrectionTransform()
    {
    }

    public CorrectionTransform(CorrectionMode mode)
    {
        Mode = mode;
    }

    public CorrectionMode Mode { get; private set; }

    public override bool IsNull => Mode == CorrectionMode.None;
}