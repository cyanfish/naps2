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
        Pane.ViewInPreviewPaneRequested += (_, _) =>
        {
            CurrentImage = Pane.CurrentImage!;
            EditorType = Pane.ActiveEditorType;
            ContinueInPane = true;
            Close();
        };
        EtoPlatform.Current.HandleKeyDown(this, Pane.HandleKeyDown);
    }

    public PreviewPane Pane { get; }

    public UiImage CurrentImage
    {
        get => field ?? throw new InvalidOperationException();
        set;
    }

    public Type? EditorType { get; set; }

    /// <summary>
    /// Whether the window was closed by "View in Preview Pane".
    /// </summary>
    public bool ContinueInPane { get; private set; }

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
        Pane.Show(CurrentImage, EditorType);
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
