using System;

namespace NAPS2.Config
{
    public class ChildConfigProvider<TParent, TChild> : ConfigProvider<TChild>
    {
        private readonly ConfigProvider<TParent> _parentProvider;
        private readonly Func<TParent, TChild> _childSelector;

        public ChildConfigProvider(ConfigProvider<TParent> parentProvider, Func<TParent, TChild> childSelector)
        {
            _parentProvider = parentProvider;
            _childSelector = childSelector;
        }

        protected override T GetInternal<T>(Func<TChild, T> func) => _parentProvider.Get(parent =>
        {
            var child = _childSelector(parent) ?? throw new InvalidOperationException("Child config should not be null");
            return func(child);
        });
    }
}
