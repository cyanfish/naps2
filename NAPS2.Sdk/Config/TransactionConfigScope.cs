using System;

namespace NAPS2.Config
{
    public class TransactionConfigScope<TConfig> : ConfigScope<TConfig>
    {
        private readonly ConfigScope<TConfig> store;
        private readonly Func<TConfig> factory;
        private TConfig changes;

        public TransactionConfigScope(ConfigScope<TConfig> store, Func<TConfig> factory) : base(ConfigScopeMode.ReadWrite)
        {
            if (store.Mode == ConfigScopeMode.ReadOnly)
            {
                throw new ArgumentException("A transaction can't be created for a ReadOnly scope.", nameof(store));
            }
            this.store = store;
            this.factory = factory;
            changes = factory();
        }

        public bool HasChanges { get; private set; }

        public event EventHandler HasChangesChanged;

        public void Commit()
        {
            lock (this)
            {
                lock (store)
                {
                    store.SetAll(changes);
                    changes = factory();
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
            var value = func(changes);
            if (value != null)
            {
                return value;
            }
            return store.Get(func);
        }

        protected override void SetInternal(Action<TConfig> func)
        {
            func(changes);
            if (!HasChanges)
            {
                HasChanges = true;
                HasChangesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void SetAllInternal(TConfig delta)
        {
            ConfigCopier.Copy(delta, changes);
        }
    }
}
