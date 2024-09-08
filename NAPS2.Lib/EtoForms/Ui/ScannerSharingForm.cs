using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Remoting.Server;

namespace NAPS2.EtoForms.Ui;

public class ScannerSharingForm : EtoDialogBase
{
    private readonly ISharedDeviceManager _sharedDeviceManager;
    private readonly IOsServiceManager _osServiceManager;
    private readonly ErrorOutput _errorOutput;

    private readonly CheckBox _shareAsService = C.CheckBox(UiStrings.ShareAsService);
    private readonly IListView<SharedDevice> _listView;

    private readonly ActionCommand _addCommand;
    private readonly ActionCommand _editCommand;
    private readonly ActionCommand _deleteCommand;

    private bool _suppressChangeEvent;

    public ScannerSharingForm(Naps2Config config, SharedDevicesListViewBehavior listViewBehavior,
        ISharedDeviceManager sharedDeviceManager, IOsServiceManager osServiceManager, ErrorOutput errorOutput,
        IIconProvider iconProvider)
        : base(config)
    {
        Title = UiStrings.ScannerSharingFormTitle;
        IconName = "wireless_small";

        _sharedDeviceManager = sharedDeviceManager;
        _osServiceManager = osServiceManager;
        _errorOutput = errorOutput;

        _listView = EtoPlatform.Current.CreateListView(listViewBehavior);
        _addCommand = new ActionCommand(DoAdd)
        {
            MenuText = UiStrings.Share,
            IconName = "add_small"
        };
        _editCommand = new ActionCommand(DoEdit)
        {
            MenuText = UiStrings.Edit,
            IconName = "pencil_small"
        };
        _deleteCommand = new ActionCommand(DoDelete)
        {
            MenuText = UiStrings.Delete,
            IconName = "cross_small"
        };

        var sharingKsm = new KeyboardShortcutManager();
        sharingKsm.Assign("Esc", Close);
        sharingKsm.Assign("Del", _deleteCommand);
        EtoPlatform.Current.HandleKeyDown(_listView.Control, sharingKsm.Perform);

        // TODO: Enable
        // _shareAsService.Checked = _osServiceManager.IsRegistered;
        // _shareAsService.CheckedChanged += ShareAsServiceCheckedChanged;
        EtoPlatform.Current.AttachDpiDependency(this, _ => _listView.RegenerateImages());
        _listView.ImageSize = new Size(48, 48);
        _listView.SelectionChanged += SelectionChanged;

        _addCommand.Enabled = true;
        _editCommand.Enabled = false;
        _deleteCommand.Enabled = false;

        var contextMenu = new ContextMenu();
        _listView.ContextMenu = contextMenu;
        contextMenu.AddItems(
            C.ButtonMenuItem(this, _editCommand),
            C.ButtonMenuItem(this, _deleteCommand));
        contextMenu.Opening += ContextMenuOpening;
    }

    protected override void BuildLayout()
    {
        FormStateController.DefaultExtraLayoutSize = new Size(200, 0);
        base.BuildLayout();

        LayoutController.Content = L.Column(
            C.Label(UiStrings.ScannerSharingIntro).DynamicWrap(400),
            // _shareAsService,
            C.Spacer(),
            _listView.Control.Scale().NaturalHeight(80),
            L.Row(
                L.Column(
                    L.Row(
                        C.Button(_addCommand, ButtonImagePosition.Left),
                        C.Button(_editCommand, ButtonImagePosition.Left),
                        C.Button(_deleteCommand, ButtonImagePosition.Left)
                    )
                ),
                C.Filler(),
                C.CancelButton(this, UiStrings.Done)
            ));
    }

    public Action<ProcessedImage>? ImageCallback { get; set; }

    private SharedDevice? SelectedDevice => _listView.Selection.SingleOrDefault();

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ReloadDevices();
    }

    private void ReloadDevices()
    {
        _listView.SetItems(_sharedDeviceManager.SharedDevices);
    }

    private void SelectionChanged(object? sender, EventArgs e)
    {
        _editCommand.Enabled = _listView.Selection.Count == 1;
        _deleteCommand.Enabled = _listView.Selection.Count > 0;
    }

    private void ContextMenuOpening(object? sender, EventArgs e)
    {
        _editCommand.Enabled = SelectedDevice != null;
        _deleteCommand.Enabled = SelectedDevice != null;
    }

    private void ShareAsServiceCheckedChanged(object? sender, EventArgs e)
    {
        if (_suppressChangeEvent) return;
        _suppressChangeEvent = true;
        try
        {
            if (_shareAsService.IsChecked())
            {
                _osServiceManager.Register();
            }
            else
            {
                _osServiceManager.Unregister();
            }
        }
        catch (Exception ex)
        {
            // TODO: Maybe we display a generic string here?
            Log.ErrorException(ex.Message, ex);
            _errorOutput.DisplayError(ex.Message, ex);
            _shareAsService.Checked = _osServiceManager.IsRegistered;
        }
        finally
        {
            _suppressChangeEvent = false;
        }
    }

    private void DoAdd()
    {
        var fedit = FormFactory.Create<SharedDeviceForm>();
        fedit.ShowModal();
        if (fedit.Result)
        {
            _sharedDeviceManager.AddSharedDevice(fedit.SharedDevice!);
            ReloadDevices();
        }
    }

    private void DoEdit()
    {
        var originalDevice = SelectedDevice;
        if (originalDevice != null)
        {
            var fedit = FormFactory.Create<SharedDeviceForm>();
            fedit.SharedDevice = originalDevice;
            fedit.ShowModal();
            if (fedit.Result)
            {
                _sharedDeviceManager.ReplaceSharedDevice(originalDevice, fedit.SharedDevice!);
                ReloadDevices();
            }
        }
    }

    private void DoDelete()
    {
        if (SelectedDevice != null)
        {
            string message = string.Format(UiStrings.ConfirmDeleteSharedDevice, SelectedDevice.Name);
            if (MessageBox.Show(message, MiscResources.Delete, MessageBoxButtons.OKCancel, MessageBoxType.Warning,
                    MessageBoxDefaultButton.OK) == DialogResult.Ok)
            {
                _sharedDeviceManager.RemoveSharedDevice(SelectedDevice);
                ReloadDevices();
            }
        }
    }
}