using Eto.Forms;

namespace NAPS2.EtoForms;

public abstract class EtoPlatform
{
    private static EtoPlatform? _current;

    public static EtoPlatform Current
    {
        get => _current ?? throw new InvalidOperationException();
        set => _current = value ?? throw new ArgumentNullException(nameof(value));
    }

    public abstract IListView<T> CreateListView<T>(ListViewBehavior<T> behavior);
    public abstract void ConfigureImageButton(Button button);
}