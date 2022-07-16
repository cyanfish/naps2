namespace NAPS2.WinForms;

public class DesktopFormProvider
{
    private FormBase? _desktopForm;

    public FormBase DesktopForm
    {
        get => _desktopForm ?? throw new InvalidOperationException();
        set => _desktopForm = value;
    }
}