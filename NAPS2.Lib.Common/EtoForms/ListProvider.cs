using System.Collections.Immutable;

namespace NAPS2.EtoForms;

public class ListProvider<T>
{
    private ImmutableList<T> _value = ImmutableList<T>.Empty;

    public ImmutableList<T> Value
    {
        get => _value;
        set
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
            OnChanged?.Invoke();
        }
    }

    public event Action? OnChanged;
}