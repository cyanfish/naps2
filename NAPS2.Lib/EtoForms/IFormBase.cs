using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms;

public interface IFormBase
{
    FormStateController FormStateController { get; }

    IFormFactory FormFactory { get; set; }

    Naps2Config Config { get; set; }

    LayoutController LayoutController { get; }

    IntPtr NativeHandle { get; }
}