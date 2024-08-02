using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Lang;
using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.EtoForms.Ui;

public class SelectDeviceForm : EtoDialogBase
{
    private readonly ScanDevice AlwaysAskMarker = new(Driver.Default, "*always*ask*", UiStrings.AlwaysAsk);

    private readonly RadioButton _wiaDriver;
    private readonly RadioButton _twainDriver;
    private readonly RadioButton _appleDriver;
    private readonly RadioButton _saneDriver;
    private readonly RadioButton _esclDriver;
    private readonly IIconProvider _iconProvider;
    private readonly DeviceListViewBehavior _deviceListViewBehavior;
    private readonly ScanningContext _scanningContext;
    private readonly LayoutVisibility _textListVis = new(false);
    private readonly ListBox _deviceTextList = new();
    private readonly IListView<ScanDevice> _deviceIconList;
    private readonly Button _selectDevice;
    // TODO: The spinner doesn't seem to animate on WinForms
    private readonly Spinner _spinner = new() { Enabled = true };
    private readonly ImageView _statusIcon = new();
    private readonly Label _statusLabel = new() { Text = UiStrings.SearchingForDevices };
    private readonly LayoutVisibility _spinnerVis = new(true);

    private CancellationTokenSource? _getDevicesCts;
    private Driver? _activeQuery;

    public SelectDeviceForm(Naps2Config config, IIconProvider iconProvider,
        DeviceListViewBehavior deviceListViewBehavior, ScanningContext scanningContext) : base(config)
    {
        _iconProvider = iconProvider;
        _deviceListViewBehavior = deviceListViewBehavior;
        _scanningContext = scanningContext;
        _selectDevice = C.OkButton(this, SelectDevice, UiStrings.Select);
        _deviceIconList = EtoPlatform.Current.CreateListView(deviceListViewBehavior);
        _deviceIconList.ImageSize = new Size(48, 32);
        deviceListViewBehavior.SetImage(AlwaysAskMarker, iconProvider.GetIcon("ask")!);

        _deviceTextList.Activated += (_, _) => _selectDevice.PerformClick();
        _deviceIconList.ItemClicked += (_, _) => _selectDevice.PerformClick();

        _wiaDriver = new RadioButton { Text = UiStrings.WiaDriver };
        _twainDriver = new RadioButton(_wiaDriver) { Text = UiStrings.TwainDriver };
        _appleDriver = new RadioButton(_wiaDriver) { Text = UiStrings.AppleDriver };
        _saneDriver = new RadioButton(_wiaDriver) { Text = UiStrings.SaneDriver };
        _esclDriver = new RadioButton(_wiaDriver) { Text = UiStrings.EsclDriver };

        _textListVis.IsVisible = config.Get(c => c.DeviceListAsTextOnly);
    }

    private void Driver_MouseUp(object? sender, EventArgs e)
    {
        QueryForDevices();
    }

    private void Driver_CheckedChanged(object? sender, EventArgs e)
    {
        QueryForDevices();
    }

    private Driver DeviceDriver
    {
        get => _twainDriver.Checked ? Driver.Twain
            : _wiaDriver.Checked ? Driver.Wia
            : _appleDriver.Checked ? Driver.Apple
            : _saneDriver.Checked ? Driver.Sane
            : _esclDriver.Checked ? Driver.Escl
            : ScanOptionsValidator.SystemDefaultDriver;
        set
        {
            if (value == Driver.Twain)
            {
                _twainDriver.Checked = true;
            }
            else if (value == Driver.Wia)
            {
                _wiaDriver.Checked = true;
            }
            else if (value == Driver.Apple)
            {
                _appleDriver.Checked = true;
            }
            else if (value == Driver.Sane)
            {
                _saneDriver.Checked = true;
            }
            else if (value == Driver.Escl)
            {
                _esclDriver.Checked = true;
            }
        }
    }

    protected override void BuildLayout()
    {
        // TODO: Don't show if only one driver is available
        var driverElements = new List<LayoutElement>();
        if (PlatformCompat.System.IsWiaDriverSupported)
        {
            driverElements.Add(_wiaDriver.Scale());
        }
        if (PlatformCompat.System.IsTwainDriverSupported)
        {
            driverElements.Add(_twainDriver.Scale());
        }
        if (PlatformCompat.System.IsAppleDriverSupported)
        {
            driverElements.Add(_appleDriver.Scale());
        }
        if (PlatformCompat.System.IsSaneDriverSupported)
        {
            driverElements.Add(_saneDriver.Scale());
        }
        if (PlatformCompat.System.IsEsclDriverSupported)
        {
            driverElements.Add(_esclDriver.Scale());
        }

        Title = TranslationMigrator.PickTranslated(UiStrings.ResourceManager, nameof(UiStrings.SelectSource),
            nameof(UiStrings.SelectDevice));

        FormStateController.SaveFormState = FormStateController.RestoreFormState = true;
        FormStateController.DefaultExtraLayoutSize = new Size(50, 0);

        LayoutController.Content = L.Column(
            L.Row(
                [
                    ..driverElements,
                    C.IconButton(_iconProvider.GetIcon("large_tiles_small")!, () => SetListView(false))
                        .Visible(_textListVis),
                    C.IconButton(_iconProvider.GetIcon("text_align_justify_small")!, () => SetListView(true))
                        .Visible(!_textListVis)
                ]
            ),
            _deviceIconList.Control.NaturalSize(150, 100).Scale().Visible(!_textListVis),
            _deviceTextList.NaturalSize(150, 100).Scale().Visible(_textListVis),
            L.Row(
                _spinner.Visible(_spinnerVis).AlignCenter(),
                _statusIcon.Visible(!_spinnerVis).AlignCenter(),
                _statusLabel.AlignCenter(),
                C.Filler(),
                L.OkCancel(_selectDevice, C.CancelButton(this))
            )
        );
    }

    private void SetListView(bool value)
    {
        _textListVis.IsVisible = value;
        Config.User.Set(c => c.DeviceListAsTextOnly, value);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        DeviceDriver = ScanOptions!.Driver;

        _wiaDriver.CheckedChanged += Driver_CheckedChanged;
        _twainDriver.CheckedChanged += Driver_CheckedChanged;
        _appleDriver.CheckedChanged += Driver_CheckedChanged;
        _saneDriver.CheckedChanged += Driver_CheckedChanged;
        _esclDriver.CheckedChanged += Driver_CheckedChanged;
        // TODO: Maybe have a refresh button instead? As part of the status indicator?
        _wiaDriver.MouseUp += Driver_MouseUp;
        _twainDriver.MouseUp += Driver_MouseUp;
        _appleDriver.MouseUp += Driver_MouseUp;
        _saneDriver.MouseUp += Driver_MouseUp;
        _esclDriver.MouseUp += Driver_MouseUp;

        QueryForDevices();
    }

    private void QueryForDevices()
    {
        if (_activeQuery == DeviceDriver)
        {
            return;
        }

        _deviceIconList.ImageSize = DeviceDriver == Driver.Escl ? new Size(64, 64) : new Size(48, 32);

        _getDevicesCts?.Cancel();
        _spinnerVis.IsVisible = true;
        _statusLabel.Text = UiStrings.SearchingForDevices;
        _activeQuery = DeviceDriver;

        DeviceList = new List<ScanDevice>();
        ExtraItems = new List<ScanDevice>();
        if (AllowAlwaysAsk && DeviceDriver is not (Driver.Wia or Driver.Twain))
        {
            ExtraItems.Add(AlwaysAskMarker);
        }

        UpdateDevices(true);

        var cts = new CancellationTokenSource();
        _getDevicesCts = cts;
        var optionsWithDriver = ScanOptions!.Clone();
        optionsWithDriver.Driver = DeviceDriver;
        var controller = new ScanController(_scanningContext);

        Task.Run(async () =>
        {
            try
            {
                await foreach (var device in controller.GetDevices(optionsWithDriver, cts.Token))
                {
                    Invoker.Current.Invoke(() =>
                    {
                        if (!cts.IsCancellationRequested)
                        {
                            DeviceList.Add(device);
                            UpdateDevices(false);
                            if (DeviceList.Count == 1)
                            {
                                _deviceIconList.Selection = ListSelection.Of(DeviceList[0]);
                                _deviceTextList.SelectedIndex = 0;
                            }
                        }
                    });
                }
                Invoker.Current.Invoke(() =>
                {
                    if (AllowAlwaysAsk && DeviceDriver is Driver.Wia or Driver.Twain)
                    {
                        if (!cts.IsCancellationRequested)
                        {
                            ExtraItems.Add(AlwaysAskMarker);
                            UpdateDevices(false);
                        }
                    }
                });
                Invoker.Current.Invoke(() =>
                {
                    if (!cts.IsCancellationRequested)
                    {
                        _spinnerVis.IsVisible = false;
                        _statusIcon.Image =
                            DeviceList.Count > 0
                                ? Icons.accept_small.ToEtoImage()
                                : Icons.exclamation_small.ToEtoImage();
                        _statusLabel.Text = DeviceList.Count switch
                        {
                            > 1 => string.Format(UiStrings.DevicesFound, DeviceList.Count),
                            1 => UiStrings.DeviceFoundSingular,
                            _ => UiStrings.NoDevicesFound
                        };
                    }
                });
            }
            catch (Exception ex)
            {
                Invoker.Current.Invoke(() =>
                {
                    if (!cts.IsCancellationRequested)
                    {
                        _spinnerVis.IsVisible = false;
                        _statusIcon.Image = Icons.exclamation_small.ToEtoImage();
                        _statusLabel.Text = ex.Message;
                    }
                });
            }
            finally
            {
                Invoker.Current.Invoke(() =>
                {
                    if (!cts.IsCancellationRequested)
                    {
                        _activeQuery = null;
                    }
                });
            }
        });
    }

    private void UpdateDevices(bool clear)
    {
        _deviceIconList.SetItems(DeviceList!.Concat(ExtraItems!));
        if (clear)
        {
            _deviceTextList.Items.Clear();
        }
        foreach (var device in DeviceList!.Concat(ExtraItems!).Skip(_deviceTextList.Items.Count))
        {
            _deviceTextList.Items.Add(new ListItem
            {
                Key = device.ID,
                Text = device.Name
            });
        }
    }

    public ScanOptions? ScanOptions { get; set; }

    public bool AllowAlwaysAsk { get; set; }

    public List<ScanDevice>? DeviceList { get; private set; }

    private List<ScanDevice>? ExtraItems { get; set; }

    public DeviceChoice Choice { get; private set; } = DeviceChoice.None;

    private bool SelectDevice()
    {
        if (_textListVis.IsVisible)
        {
            if (_deviceTextList.SelectedValue == null)
            {
                _deviceTextList.Focus();
                return false;
            }
            Choice = DeviceChoice.ForDevice(DeviceList!.First(x => x.ID == _deviceTextList.SelectedKey));
        }
        else
        {
            if (_deviceIconList.Selection.Count == 0)
            {
                _deviceIconList.Control.Focus();
                return false;
            }
            Choice = DeviceChoice.ForDevice(_deviceIconList.Selection.First());
        }
        if (Choice.Device == AlwaysAskMarker)
        {
            Choice = DeviceChoice.ForAlwaysAsk(DeviceDriver);
        }
        return true;
    }
}