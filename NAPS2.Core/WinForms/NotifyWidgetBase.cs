using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class NotifyWidgetBase : UserControl
    {
        public event EventHandler HideNotify;

        protected void InvokeHideNotify()
        {
            SafeInvoke(() => HideNotify?.Invoke(this, new EventArgs()));
        }

        public virtual void ShowNotify()
        {
        }

        public void Invoke(Action action)
        {
            ((Control)this).Invoke(action);
        }

        public void SafeInvoke(Action action)
        {
            try
            {
                Invoke(action);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        public virtual NotifyWidgetBase Clone() => throw new NotImplementedException();
    }
}
