using NAPS2.EtoForms.Ui;
using NAPS2.Scan;

namespace NAPS2.EtoForms;

public class EtoDevicePrompt : IDevicePrompt
{
    private readonly IFormFactory _formFactory;

    public EtoDevicePrompt(IFormFactory formFactory)
    {
        _formFactory = formFactory;
    }

    public Task<DeviceChoice> PromptForDevice(ScanOptions options, bool allowAlwaysAsk)
    {
        // TODO: Extension method or something to turn InvokeGet into Task<T>?
        return Task.FromResult(Invoker.Current.InvokeGet(() =>
        {
            var deviceForm = _formFactory.Create<SelectDeviceForm>();
            deviceForm.ScanOptions = options;
            deviceForm.AllowAlwaysAsk = allowAlwaysAsk;
            deviceForm.ShowModal();
            return deviceForm.Choice;
        }));
    }
}