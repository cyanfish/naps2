using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Widgets;

public class DeviceSelectorWidget
{
    private readonly IScanPerformer _scanPerformer;
    private readonly DeviceCapsCache _deviceCapsCache;
    private readonly IIconProvider _iconProvider;
    private readonly IFormBase _parentWindow;

    private readonly ImageView _deviceIcon = new();
    private readonly Label _deviceName = new();
    private readonly Label _deviceDriver = new();
    private readonly LayoutVisibility _deviceVis = new(false);
    private readonly Button _chooseDevice = new() { Text = UiStrings.ChooseDevice };

    private DeviceChoice _choice = DeviceChoice.None;
    private Image? _deviceIconImage;
    private string _deviceIconName = "device";
    private CancellationTokenSource? _loadIconCts;

    public DeviceSelectorWidget(IScanPerformer scanPerformer, DeviceCapsCache deviceCapsCache,
        IIconProvider iconProvider, IFormBase parentWindow)
    {
        _scanPerformer = scanPerformer;
        _deviceCapsCache = deviceCapsCache;
        _iconProvider = iconProvider;
        _parentWindow = parentWindow;
        _chooseDevice.Click += ChooseDevice;
        EtoPlatform.Current.AttachDpiDependency(_deviceIcon, _ => UpdateDeviceIconImage());
    }

    public required Func<ScanProfile> ProfileFunc { get; init; }

    public bool AllowAlwaysAsk { get; init; }

    public DeviceChoice Choice
    {
        get => _choice;
        set
        {
            _choice = value;
            if (value == DeviceChoice.None)
            {
                _deviceName.Text = "";
                _deviceVis.IsVisible = false;
            }
            else
            {
                _deviceName.Text = value.Device?.Name ?? UiStrings.AlwaysAsk;
                _deviceDriver.Text = value.Driver switch
                {
                    Driver.Wia => UiStrings.WiaDriver,
                    Driver.Twain => UiStrings.TwainDriver,
                    Driver.Sane => UiStrings.SaneDriver,
                    Driver.Escl => UiStrings.EsclDriver,
                    Driver.Apple => UiStrings.AppleDriver,
                    _ => ""
                };
                _deviceVis.IsVisible = true;
                SetDeviceIcon(Choice.Device?.IconUri);
            }
        }
    }

    public bool Enabled
    {
        get => _chooseDevice.Enabled;
        set => _chooseDevice.Enabled = value;
    }

    public bool ShowChooseDevice { get; set; } = true;

    public event EventHandler<DeviceChangedEventArgs>? DeviceChanged;

    private async void ChooseDevice(object? sender, EventArgs args)
    {
        var choice = await _scanPerformer.PromptForDevice(ProfileFunc(), AllowAlwaysAsk, _parentWindow.NativeHandle);
        if (choice.Device != null || choice.AlwaysAsk)
        {
            var previousChoice = Choice;
            Choice = choice;
            SetDeviceIcon(Choice.Device?.IconUri);
            DeviceChanged?.Invoke(this,
                new DeviceChangedEventArgs { PreviousChoice = previousChoice, NewChoice = choice });
        }
    }

    public void SetDeviceIcon(string? iconUri)
    {
        _deviceIconImage = _deviceCapsCache.GetCachedIcon(iconUri);
        _deviceIconName = _choice.AlwaysAsk ? "ask" : "device";
        UpdateDeviceIconImage();

        if (((Window) _parentWindow).Loaded)
        {
            _parentWindow.LayoutController.Invalidate();
        }
        if (_deviceIconImage == null && iconUri != null)
        {
            ReloadDeviceIcon(iconUri);
        }
    }

    private void ReloadDeviceIcon(string iconUri)
    {
        var cts = new CancellationTokenSource();
        _loadIconCts?.Cancel();
        _loadIconCts = cts;
        Task.Run(async () =>
        {
            var icon = await _deviceCapsCache.LoadIcon(iconUri);
            if (icon != null)
            {
                Invoker.Current.Invoke(() =>
                {
                    if (!cts.IsCancellationRequested)
                    {
                        _deviceIconImage = icon;
                        UpdateDeviceIconImage();
                        _parentWindow.LayoutController.Invalidate();
                    }
                });
            }
        });
    }

    private void UpdateDeviceIconImage()
    {
        float scale = EtoPlatform.Current.GetScaleFactor(_deviceIcon.ParentWindow);
        _deviceIcon.Image = _deviceIconImage ?? _iconProvider.GetIcon(_deviceIconName, scale);
        var size = _deviceIconImage != null ? new SizeF(48, 48) : new SizeF(32, 32);
        _deviceIcon.Size = Size.Round(size * EtoPlatform.Current.GetLayoutScaleFactor(_deviceIcon.ParentWindow));
    }

    public static implicit operator LayoutElement(DeviceSelectorWidget control)
    {
        return control.AsControl();
    }

    public LayoutElement AsControl()
    {
        return L.GroupBox(UiStrings.DeviceLabel,
            L.Row(
                _deviceIcon.Visible(_deviceVis).AlignCenter().NaturalWidth(48),
                L.Column(
                    C.Filler(),
                    ShowChooseDevice ? _deviceName.DynamicWrap(300) : _deviceName.Ellipsize().MaxWidth(150),
                    _deviceDriver,
                    C.Filler()
                ).Spacing(5).Visible(_deviceVis).Scale(),
                // TODO: We can consider a compact choose-device button for the sidebar, but maybe simpler to force
                // creation of separate profiles
                ShowChooseDevice ? _chooseDevice.AlignCenter() : C.None()
            )
        );
    }
}