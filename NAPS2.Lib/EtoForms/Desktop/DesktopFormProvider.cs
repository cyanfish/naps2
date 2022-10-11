namespace NAPS2.EtoForms.Desktop;

public class DesktopFormProvider
{
    private EtoFormBase? _desktopForm;

    public EtoFormBase DesktopForm
    {
        get => _desktopForm ?? throw new InvalidOperationException();
        set
        {
            _desktopForm = value;
            DesktopFormChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? DesktopFormChanged;
}