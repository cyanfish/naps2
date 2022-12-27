using System.Windows.Forms;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsInvoker : IInvoker
{
    private readonly Func<Form> _formFunc;

    public WinFormsInvoker(Func<Form> formFunc)
    {
        _formFunc = formFunc;
    }

    public void Invoke(Action action)
    {
        try
        {
            _formFunc().Invoke(action);
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    public void InvokeAsync(Action action)
    {
        try
        {
            _formFunc().BeginInvoke(action);
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    public T InvokeGet<T>(Func<T> func)
    {
        T value = default!;
        _formFunc().Invoke(() => value = func());
        return value;
    }
}