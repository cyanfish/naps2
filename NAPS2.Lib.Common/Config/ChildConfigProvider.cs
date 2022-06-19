namespace NAPS2.Config;

public class ChildConfigProvider<TParent, TChild> : IConfigProvider<TChild>
{
    private readonly IConfigProvider<TParent> _parentProvider;
    private readonly Func<TParent, TChild> _childSelector;

    public ChildConfigProvider(IConfigProvider<TParent> parentProvider, Func<TParent, TChild> childSelector)
    {
        _parentProvider = parentProvider;
        _childSelector = childSelector;
    }

    public T Get<T>(Func<TChild, T?> func) where T : struct => _parentProvider.Get(parent =>
    {
        var child = _childSelector(parent) ?? throw new InvalidOperationException("Child config should not be null");
        return func(child);
    });

    public T Get<T>(Func<TChild, T?> func) where T : class => _parentProvider.Get(parent =>
    {
        var child = _childSelector(parent) ?? throw new InvalidOperationException("Child config should not be null");
        return func(child);
    });
}