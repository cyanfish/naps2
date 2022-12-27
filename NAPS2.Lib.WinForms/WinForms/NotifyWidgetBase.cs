using System.Windows.Forms;

namespace NAPS2.WinForms;

public class NotifyWidgetBase : UserControl
{
    public event EventHandler? HideNotify;

    protected void InvokeHideNotify()
    {
        Invoker.Current.Invoke(() => HideNotify?.Invoke(this, EventArgs.Empty));
    }

    public virtual void ShowNotify()
    {
    }

    public virtual NotifyWidgetBase Clone() => throw new NotImplementedException();
}