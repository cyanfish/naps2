using System.Linq.Expressions;

namespace NAPS2.Config.Model;

/// <summary>
/// Represents a transaction wrapping an underlying ConfigScope. Writes are only flushed to the underlying scope when
/// Commit() is called.
///
/// This has several uses:
/// - Ensure changes to multiple config properties are atomic
/// - Reduce filesystem writes when multiple properties are changed and the underlying scope is a FileConfigScope
/// - Maintain a tentative set of changes (as in a Settings window that has an Apply button)
///
/// To create a TransactionConfigScope, use the ConfigScope.BeginTransaction() extension method.  
/// </summary>
public class TransactionConfigScope<TConfig> : ConfigScope<TConfig>
{
    private List<Action> _removeActions = new();
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

    /// <summary>
    /// Whether or not the transaction has uncommitted changes.
    /// 
    /// This can be used in a settings dialog to determine whether the "Apply" button should be enabled.
    /// </summary>
    public bool HasChanges { get; private set; }

    /// <summary>
    /// Fired when the value of HasChanges changes.
    ///
    /// This can be used in a settings dialog to trigger recalculation of whether the "Apply" button should be enabled. 
    /// </summary>e
    public event EventHandler? HasChangesChanged;

    /// <summary>
    /// Flushes all changes to the underlying scope.
    /// </summary>
    public void Commit()
    {
        lock (this)
        {
            lock (OriginalScope)
            {
                foreach (var action in _removeActions)
                {
                    action();
                }
                OriginalScope.CopyFrom(_changes);
                _changes = new();
                _removeActions = new();
            }
            ChangesFlushed();
        }
    }

    /// <summary>
    /// Resets all changes that haven't been committed.
    /// </summary>
    public void Rollback()
    {
        lock (this)
        {
            _changes = new();
            _removeActions = new();
            ChangesFlushed();
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
        _removeActions.Add(() => OriginalScope.Remove(accessor));
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

    private void ChangesFlushed()
    {
        if (HasChanges)
        {
            HasChanges = false;
            HasChangesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}