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
            Exception? error = null;
            _formFunc().Invoke(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });
            if (error != null)
            {
                error.PreserveStackTrace();
                throw error;
            }
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
            Exception? error = null;
            _formFunc().BeginInvoke(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });
            if (error != null)
            {
                error.PreserveStackTrace();
                throw error;
            }
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
        Exception? error = null;
        _formFunc().Invoke(() =>
        {
            try
            {
                value = func();
            }
            catch (Exception ex)
            {
                error = ex;
            }
            if (error != null)
            {
                error.PreserveStackTrace();
                throw error;
            }
        });
        return value;
    }
}