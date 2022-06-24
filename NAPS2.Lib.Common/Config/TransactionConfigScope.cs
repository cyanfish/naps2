using System.Linq.Expressions;

namespace NAPS2.Config;

public class TransactionConfigScope<TConfig> : ConfigScope<TConfig>
{
    private ConfigStorage<TConfig> _changes = new();

    public TransactionConfigScope(ConfigScope<TConfig> originalScope) : base(ConfigScopeMode.ReadWrite)
    {
        if (originalScope.Mode == ConfigScopeMode.ReadOnly)
        {
            throw new ArgumentException("A transaction can't be created for a ReadOnly scope.", nameof(originalScope));
        }
        OriginalScope = originalScope;
    }

    public ConfigScope<TConfig> OriginalScope { get; }

    public bool HasChanges { get; private set; }

    public event EventHandler? HasChangesChanged;

    public void Commit()
    {
        lock (this)
        {
            lock (OriginalScope)
            {
                OriginalScope.CopyFrom(_changes);
                _changes = new();
            }
            if (HasChanges)
            {
                HasChanges = false;
                HasChangesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    protected override bool TryGetInternal(ConfigLookup lookup, out object? value)
    {
        if (_changes.TryGet(lookup, out value))
        {
            return true;
        }
        return OriginalScope.TryGet(lookup, out value);
    }

    protected override void SetInternal<T>(Expression<Func<TConfig, T>> accessor, T value)
    {
        _changes.Set(accessor, value);
        ChangesMade();
    }

    protected override void RemoveInternal<T>(Expression<Func<TConfig, T>> accessor)
    {
        _changes.Remove(accessor);
        ChangesMade();
    }

    protected override void CopyFromInternal(ConfigStorage<TConfig> source)
    {
        _changes.CopyFrom(source);
        ChangesMade();
    }

    private void ChangesMade()
    {
        if (!HasChanges)
        {
            HasChanges = true;
            HasChangesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}