using System;

namespace NAPS2.Config
{
    public class TransactionConfigScope<TConfig> : ConfigScope<TConfig>
    {
        private readonly Func<TConfig> _factory;
        private TConfig _changes;

        public TransactionConfigScope(ConfigScope<TConfig> originalScope, Func<TConfig> factory) : base(ConfigScopeMode.ReadWrite)
        {
            if (originalScope.Mode == ConfigScopeMode.ReadOnly)
            {
                throw new ArgumentException("A transaction can't be created for a ReadOnly scope.", nameof(originalScope));
            }
            OriginalScope = originalScope;
            _factory = factory;
            _changes = factory();
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
                    OriginalScope.SetAll(_changes);
                    _changes = _factory();
                }
                if (HasChanges)
                {
                    HasChanges = false;
                    HasChangesChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        protected override T GetInternal<T>(Func<TConfig, T> func)
        {
            var value = func(_changes);
            if (value != null)
            {
                return value;
            }
            return OriginalScope.Get(func);
        }

        protected override void SetInternal(Action<TConfig> func)
        {
            func(_changes);
            if (!HasChanges)
            {
                HasChanges = true;
                HasChangesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void SetAllInternal(TConfig delta)
        {
            ConfigCopier.Copy(delta, _changes);
        }
    }
}
