using Eto.Drawing;
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

    public abstract IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) where T : notnull;
    public abstract void ConfigureImageButton(Button button);
    public abstract Bitmap ToBitmap(IMemoryImage image);
    public abstract IMemoryImage DrawHourglass(ImageContext imageContext, IMemoryImage thumb);
    public abstract void SetFrame(Control container, Control control, Point location, Size size);
    public abstract Control CreateContainer();
    public abstract void AddToContainer(Control container, Control control);

    public virtual SizeF GetPreferredSize(Control control, SizeF availableSpace)
    {
        return control.GetPreferredSize(availableSpace);
    }
}