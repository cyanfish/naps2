using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class ManualDuplexForm : EtoDialogBase
{
    private const int PREVIEW_THUMBNAIL_SIZE = 192;

    private readonly UiImageList _uiImageList;
    private readonly DesktopFormProvider _desktopFormProvider;
    private readonly ImageListActions _imageListActions;
    private readonly IListView<UiImage> _listView;
    private readonly CheckBox _alwaysShowPreview = new() { Text = UiStrings.AlwaysShowPreview };
    private readonly CheckBox _reverseBackSides = new() { Text = UiStrings.ReverseBackSides };
    private readonly HelpWidget _help = new() { Text = UiStrings.ManualDuplexHelp };

    public ManualDuplexForm(Naps2Config config, UiImageList uiImageList, DesktopFormProvider desktopFormProvider,
        ImageListActions imageListActions, ManualDuplexListViewBehavior listViewBehavior)
        : base(config)
    {
        _uiImageList = uiImageList;
        _desktopFormProvider = desktopFormProvider;
        _imageListActions = imageListActions;
        Title = UiStrings.ManualDuplexFormTitle;
        IconName = "column_double_small";

        _listView = EtoPlatform.Current.CreateListView(listViewBehavior);
        _alwaysShowPreview.Checked = Config.Get(c => c.ManualDuplexSettings.AlwaysShowPreview);
        _reverseBackSides.Checked = Config.Get(c => c.ManualDuplexSettings.ReverseBackSides);

        _reverseBackSides.CheckedChanged += ReverseCheckedChanged;

        EtoPlatform.Current.AttachDpiDependency(this, _ => _listView.RegenerateImages());
        _listView.ImageSize = new Size(PREVIEW_THUMBNAIL_SIZE, PREVIEW_THUMBNAIL_SIZE);
    }

    private void ReverseCheckedChanged(object? sender, EventArgs e)
    {
        ReloadPages();
    }

    protected override void BuildLayout()
    {        
        FormStateController.DefaultExtraLayoutSize = new Size(200, 0);

        LayoutController.Content = L.Column(
            _reverseBackSides,
            _alwaysShowPreview,
            C.Spacer(),
            C.Label(UiStrings.Preview),
            _listView.Control.Scale().NaturalSize(400, PREVIEW_THUMBNAIL_SIZE + 28),
            _help.Label,
            L.Row(
                _help.Button,
                C.Filler(),
                L.OkCancel(C.OkButton(this, Apply), C.CancelButton(this))
            ));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ReloadPages();
    }

    private void ReloadPages()
    {
        var images = _uiImageList.Images.ToList();
        new ListMutation<UiImage>.ManualDuplex(_reverseBackSides.IsChecked()).Apply(images,
            Selectable.Empty<UiImage>());
        _listView.SetItems(images);
    }

    private void Apply()
    {
        var transact = Config.User.BeginTransaction();
        transact.Set(c => c.ManualDuplexSettings.AlwaysShowPreview, _alwaysShowPreview.IsChecked());
        transact.Set(c => c.ManualDuplexSettings.ReverseBackSides, _reverseBackSides.IsChecked());
        transact.Commit();

        _imageListActions.ManualDuplex();

        _desktopFormProvider.DesktopForm.UpdateManualDuplex();
    }
}