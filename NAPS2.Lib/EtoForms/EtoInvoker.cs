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
        EtoPlatform.Current.Invoke(_application, action);
    }

    public void InvokeDispatch(Action action)
    {
        EtoPlatform.Current.AsyncInvoke(_application, action);
    }

    public T InvokeGet<T>(Func<T> func)
    {
        T value = default!;
        EtoPlatform.Current.Invoke(_application, () => value = func());
        return value;
    }
}