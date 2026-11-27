using Autofac;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;

namespace NAPS2;

public class AutofacImageEditorFactory(IComponentContext container) : IImageEditorFactory
{
    public T Create<T>() where T : ImageEditor => container.Resolve<T>();
}
