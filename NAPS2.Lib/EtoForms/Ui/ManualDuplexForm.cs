using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class ManualDuplexForm : EtoDialogBase
{
    private readonly UiImageList _uiImageList;
    private readonly DesktopFormProvider _desktopFormProvider;
    private readonly ImageListActions _imageListActions;
    private readonly IListView<UiImage> _listView;
    private readonly CheckBox _alwaysShowPreview = new() { Text = UiStrings.AlwaysShowPreview };
    private readonly CheckBox _reverseBackSides = new() { Text = UiStrings.ReverseBackSides };
    private readonly Label _helpLabel = new() { Text = UiStrings.ManualDuplexHelp };
    private readonly LayoutControl _helpButton;
    private readonly LayoutVisibility _helpVis = new(false);

    public ManualDuplexForm(Naps2Config config, ManualDuplexListViewBehavior listViewBehavior, UiImageList uiImageList,
        DesktopFormProvider desktopFormProvider, ImageListActions imageListActions)
        : base(config)
    {
        _uiImageList = uiImageList;
        _desktopFormProvider = desktopFormProvider;
        _imageListActions = imageListActions;
        Title = UiStrings.ManualDuplexFormTitle;
        IconName = "column_double_small";

        _listView = EtoPlatform.Current.CreateListView(listViewBehavior);
        _helpButton = C.IconButton("information_small", _helpVis.Toggle);
        _alwaysShowPreview.Checked = Config.Get(c => c.ManualDuplexSettings.AlwaysShowPreview);
        _reverseBackSides.Checked = Config.Get(c => c.ManualDuplexSettings.ReverseBackSides);

        EtoPlatform.Current.AttachDpiDependency(this, _ => _listView.RegenerateImages());
        _listView.ImageSize = new Size(128, 128);
    }

    protected override void BuildLayout()
    {
        FormStateController.DefaultExtraLayoutSize = new Size(200, 0);

        LayoutController.Content = L.Column(
            _reverseBackSides,
            _alwaysShowPreview,
            C.Spacer(),
            C.Label(UiStrings.Preview),
            _listView.Control.Scale().NaturalHeight(80),
            _helpLabel.Visible(_helpVis),
            L.Row(
                _helpButton,
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
        _listView.SetItems(_uiImageList.Images);
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