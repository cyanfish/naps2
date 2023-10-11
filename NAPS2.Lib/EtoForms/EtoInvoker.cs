using Eto.Forms;

namespace NAPS2.EtoForms;

public class EtoInvoker : IInvoker
{
    private readonly Application _application;

    public EtoInvoker(Application application)
    {
        _application = application;
    }

    public void Invoke(Action action)
    {
        _application.Invoke(action);
    }

    public void InvokeDispatch(Action action)
    {
        _application.AsyncInvoke(action);
    }

    public T InvokeGet<T>(Func<T> func)
    {
        T value = default!;
        _application.Invoke(() => value = func());
        return value;
    }
}