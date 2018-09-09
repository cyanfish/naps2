using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Util
{
    public class DeferredAction
    {
        private readonly Action action;
        private int counter;

        public DeferredAction(Action action)
        {
            this.action = action;
        }

        public bool IsDeferred => counter > 0;

        public IDisposable Defer()
        {
            return new DeferSaveObject(this);
        }

        private class DeferSaveObject : IDisposable
        {
            private readonly DeferredAction deferredAction;

            private bool disposed;

            public DeferSaveObject(DeferredAction deferredAction)
            {
                this.deferredAction = deferredAction;
                lock (deferredAction)
                {
                    deferredAction.counter += 1;
                }
            }

            public void Dispose()
            {
                lock (deferredAction)
                {
                    if (disposed) return;
                    disposed = true;

                    deferredAction.counter -= 1;
                    if (!deferredAction.IsDeferred)
                    {
                        deferredAction.action();
                    }
                }
            }
        }
    }
}
