using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class ImageEditorForm : EtoDialogBase
{
    private readonly ScrollZoomImageViewer _viewer = new();

    public ImageEditorForm(Naps2Config config, ColorScheme colorScheme) : base(config)
    {
        FormStateController.DefaultExtraLayoutSize = new Size(400, 400);
        Maximizable = true;
        _viewer.ColorScheme = colorScheme;
    }

    public ImageEditor Editor
    {
        get => field ?? throw new InvalidOperationException();
        set
        {
            field = value;
            FormStateController.FormName = value.GetType().Name;
        }
    }

    protected override void BuildLayout()
    {
        Title = Editor.Title;
        if (Editor.IconName != null)
        {
            IconName = Editor.IconName;
        }
        Editor.ParentWindow = this;
        Editor.Viewer = _viewer;
        Editor.Initialize();
        Editor.Finished += (_, _) => Close();
        LayoutController.Content = L.Column(_viewer.Scale(), Editor.CreateLayout());
        DefaultButton = Editor.OkButton;
        AbortButton = Editor.CancelButton;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _viewer.ZoomToContainer();
        Editor.OnShown();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        Editor.HandleKeyDown(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Editor.Dispose();
    }
}
