namespace NAPS2.Images;

public class Memento : IEquatable<Memento>, IDisposable
{
    private readonly List<ScannedImage.Snapshot> _snapshots;
            
    public static readonly Memento Empty = new Memento(new List<ScannedImage.Snapshot>());

    public Memento(IEnumerable<ScannedImage> images)
        : this(images.Select(x => x.Preserve()))
    {
    }

    public Memento(IEnumerable<ScannedImage.Snapshot> snapshots)
    {
        _snapshots = snapshots.ToList();
    }

    public bool Equals(Memento other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _snapshots.SequenceEqual(other._snapshots);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Memento) obj);
    }

    public override int GetHashCode() => _snapshots.GetHashCode();

    public static bool operator ==(Memento left, Memento right) => Equals(left, right);

    public static bool operator !=(Memento left, Memento right) => !Equals(left, right);

    public void Dispose()
    {
        foreach (var snapshot in _snapshots)
        {
            snapshot.Dispose();
        }
    }
}