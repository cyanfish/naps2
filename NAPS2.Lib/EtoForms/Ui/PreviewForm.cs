using Eto.Drawing;
using NAPS2.EtoForms.Desktop;

namespace NAPS2.EtoForms.Ui;

public class PreviewForm : EtoDialogBase
{
    public PreviewForm(Naps2Config config, PreviewPane pane) : base(config)
    {
        Title = UiStrings.PreviewFormTitle;
        Icon = EtoPlatform.Current.IsGtk ? new Icon(1f, Icons.scanner_128.ToEtoImage()) : Icons.favicon.ToEtoIcon();
        Maximizable = true;

        Pane = pane;
        Pane.Closed += (_, _) => Close();
        EtoPlatform.Current.HandleKeyDown(this, Pane.HandleKeyDown);
    }

    public PreviewPane Pane { get; }

    public UiImage CurrentImage
    {
        get => field ?? throw new InvalidOperationException();
        set;
    }

    protected override void BuildLayout()
    {
        FormStateController.AutoLayoutSize = false;
        FormStateController.DefaultClientSize = new Size(800, 600);

        LayoutController.RootPadding = 0;
        LayoutController.Content = Pane.CreateView(this, true);
    }

    protected override void OnLoad(EventArgs eventArgs)
    {
        base.OnLoad(eventArgs);

        CreateToolbar();
        Pane.Show(CurrentImage);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Pane.ImageViewer.ZoomToContainer();
    }

    protected virtual void CreateToolbar()
    {
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Pane.Dispose();
    }
}
