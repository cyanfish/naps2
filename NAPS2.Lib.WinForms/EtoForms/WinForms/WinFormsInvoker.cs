using System.Windows.Forms;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsInvoker : IInvoker
{
    private readonly ApplicationContext _appContext;

    public WinFormsInvoker(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public void Invoke(Action action)
    {
        _appContext.MainForm.Invoke(action);
    }

    // TODO: Maybe these can be extension methods?
    public T InvokeGet<T>(Func<T> func)
    {
        T value = default;
        Invoke(() => value = func());
        return value;
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
}