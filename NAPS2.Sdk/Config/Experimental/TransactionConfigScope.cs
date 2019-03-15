using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public class TransactionConfigScope : ConfigScope
    {
        private readonly ConfigScope store;
        private CommonConfig changes;

        public TransactionConfigScope(ConfigScope store) : base(ConfigScopeMode.ReadWrite)
        {
            if (store.Mode == ConfigScopeMode.ReadOnly)
            {
                throw new ArgumentException("A transaction can't be created for a ReadOnly scope.", nameof(store));
            }
            this.store = store;
            changes = new CommonConfig();
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
                    changes = new CommonConfig();
                }
                if (HasChanges)
                {
                    HasChanges = false;
                    HasChangesChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        protected override T GetInternal<T>(Func<CommonConfig, T> func)
        {
            var value = func(changes);
            if (value != null)
            {
                return value;
            }
            return store.Get(func);
        }

        protected override void SetInternal(Action<CommonConfig> func)
        {
            func(changes);
            if (!HasChanges)
            {
                HasChanges = true;
                HasChangesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public override void SetAllInternal(CommonConfig delta)
        {
            ConfigCopier.Copy(delta, changes);
        }
    }
}
