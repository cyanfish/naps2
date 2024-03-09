namespace NAPS2.Images.Transforms;

public record CorrectionTransform : Transform
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

    public override bool CanSimplify(Transform other) => (other as CorrectionTransform)?.Mode == Mode;

    public override Transform Simplify(Transform other)
    {
        if ((other as CorrectionTransform)?.Mode != Mode)
        {
            throw new InvalidOperationException();
        }
        // It's not technically correct to say that this transform is idempotent, but in practice if you run it twice in
        // a row we probably only want it to be applied once.
        return this;
    }
}