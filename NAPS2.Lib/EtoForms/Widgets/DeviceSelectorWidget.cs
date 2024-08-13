using System.Threading;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Widgets;

public class DeviceSelectorWidget
{
    private readonly IScanPerformer _scanPerformer;
    private readonly DeviceCapsCache _deviceCapsCache;
    private readonly IFormBase _parentWindow;

    private readonly ImageView _deviceIcon = new();
    private readonly Label _deviceName = new();
    private readonly Label _deviceDriver = new();
    private readonly LayoutVisibility _deviceVis = new(false);
    private readonly Button _chooseDevice = new() { Text = UiStrings.ChooseDevice };

    private DeviceChoice _choice = DeviceChoice.None;
    private CancellationTokenSource? _loadIconCts;

    public DeviceSelectorWidget(IScanPerformer scanPerformer, DeviceCapsCache deviceCapsCache, IFormBase parentWindow)
    {
        _scanPerformer = scanPerformer;
        _deviceCapsCache = deviceCapsCache;
        _parentWindow = parentWindow;
        _chooseDevice.Click += ChooseDevice;
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
    {;
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
        var cachedIcon = _deviceCapsCache.GetCachedIcon(iconUri);
        _deviceIcon.Image =
            cachedIcon ?? (_choice.AlwaysAsk ? Icons.ask.ToEtoImage() : Icons.device.ToEtoImage());
        if (((Window) _parentWindow).Loaded)
        {
            _parentWindow.LayoutController.Invalidate();
        }
        if (cachedIcon == null && iconUri != null)
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
                        _deviceIcon.Image = icon;
                        _parentWindow.LayoutController.Invalidate();
                    }
                });
            }
        });
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
                // TODO: We should probably have a compact choose-device button for the sidebar.
                // It should also change the name of the profile if it matches the device name.
                // i.e. for users that are naming their own profiles, its their responsibility to keep the devices
                // matched up. For a "basic" user that might only create one profile, its name should keep matched
                // with the device.
                ShowChooseDevice ? _chooseDevice.AlignCenter() : C.None()
            )
        );
    }
}