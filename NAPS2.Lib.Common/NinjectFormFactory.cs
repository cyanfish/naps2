using NAPS2.Config;
using NAPS2.EtoForms;
using NAPS2.WinForms;
using Ninject;

namespace NAPS2;

public class NinjectFormFactory : IFormFactory
{
    private readonly IKernel _kernel;

    public NinjectFormFactory(IKernel kernel)
    {
        _kernel = kernel;
    }

    public T Create<T>() where T : IFormBase
    {
        var form = _kernel.Get<T>();
        form.FormFactory = _kernel.Get<IFormFactory>();
        form.Config = _kernel.Get<ScopedConfig>();
        return form;
    }
}